using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NpDirectory.Application.Common;
using NpDirectory.Application.Exceptions;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Responses;
using NpDirectory.Domain.Models;

namespace NpDirectory.Application.Services;

public class NaturalPersonsService : INaturalPersonsService
{
    private readonly IUnitOfWork _uof;
    private readonly IFileService _fileService;
    private readonly IStringLocalizer _localizer;

    public NaturalPersonsService(IUnitOfWork uof, IFileService fileService, IStringLocalizer localizer)
    {
        _uof = uof;
        _fileService = fileService;
        _localizer = localizer;
    }

    public async Task<GetNaturalPersonInfoResponse> GetNaturalPersonInfoAsync(int id)
    {
        var personInfo = await _uof.NaturalPersonsRepository.GetNaturalPersonInfoAsync(id);
        if(personInfo == null)
            throw new NotFoundException(_localizer["Error.NaturalPerson.NotFound"]);
        
        var response = new GetNaturalPersonInfoResponse
        {
            PersonInfo = personInfo.PersonInfo,
            RelatedPersons = personInfo.RelatedPersons
        };
        
        return response;
    }

    public async Task<SearchNaturalPersonsResponse> SearchNaturalPersonsAsync(SearchNaturalPersonsRequest request)
    {
        var query = await _uof.NaturalPersonsRepository.GetManyByFilterAsync(x => 
            (string.IsNullOrEmpty(request.FirstName) || x.FirstName.Contains(request.FirstName)) &&
            (string.IsNullOrEmpty(request.LastName) || x.LastName.Contains(request.LastName)) &&
            (request.Sex == default || x.Sex == request.Sex) &&
            (string.IsNullOrEmpty(request.PersonalNumber) || x.PersonalNumber.Contains(request.PersonalNumber)) &&
            (request.BirthDate == default || x.BirthDate == request.BirthDate) &&
            (request.CityId == default || x.CityId == request.CityId) &&
            (string.IsNullOrEmpty(request.PhoneNumber) || x.PhoneNumbers.Any(pn => pn.Number.Contains(request.PhoneNumber)))
        , request.Page, request.PageSize);

        var response = new SearchNaturalPersonsResponse
        {
            Persons = query.Select(np => new PersonInfoModel()
            {
                Id = np.Id,
                FirstName = np.FirstName,
                LastName = np.LastName,
                PersonalNumber = np.PersonalNumber,
                BirthDate = np.BirthDate,
                City = np.City,
                ImageUrl = np.ImageUrl,
                Sex = np.Sex,
                PhoneNumbers = np.PhoneNumbers?.Select(pn => new PhoneNumberModel
                {
                    Number = pn.Number,
                    Type = pn.Type
                }).ToList() ?? []
            }).ToList()
        };
        
        return response;
    }

    public async Task<FastSearchNaturalPersonsResponse> FastSearchNaturalPersonAsync(FastSearchNaturalPersonRequest request)
    {
        var query = await _uof.NaturalPersonsRepository.GetManyByFilterAsync(x =>
                (string.IsNullOrEmpty(request.FirstName) || EF.Functions.Like(x.FirstName.ToLower(), $"%{request.FirstName.ToLower()}%")) &&
                (string.IsNullOrEmpty(request.LastName) || EF.Functions.Like(x.LastName.ToLower(), $"%{request.LastName.ToLower()}%")) &&
                (string.IsNullOrEmpty(request.PersonalNumber) || EF.Functions.Like(x.PersonalNumber.ToLower(), $"%{request.PersonalNumber.ToLower()}%")),
            request.Page, request.PageSize);

        var response = new FastSearchNaturalPersonsResponse
        {
            Persons = query.Select(np => new PersonInfoModel()
            {
                Id = np.Id,
                FirstName = np.FirstName,
                LastName = np.LastName,
                PersonalNumber = np.PersonalNumber,
                BirthDate = np.BirthDate,
                City = np.City,
                ImageUrl = np.ImageUrl,
                Sex = np.Sex,
                PhoneNumbers = np.PhoneNumbers?.Select(pn => new PhoneNumberModel
                {
                    Number = pn.Number,
                    Type = pn.Type
                }).ToList() ?? []
            }).ToList()
        };
        
        return response;
    }

    public async Task<GenerateReportResponse> GenerateReportAsync(GenerateReportRequest request)
    {
        var relations =
            await _uof.RelationsesRepository.GetManyByFilterAsync(
                x => true,
                request.Page,
                request.PageSize,
                x => x.NaturalPerson,
                x => x.RelatedPerson);
        
        var reportPersons = new List<ReportPerson>();
        var uniquePersons = relations
            .Select(r => new List<NaturalPerson>() { r.NaturalPerson, r.RelatedPerson })
            .SelectMany(x => x)
            .DistinctBy(x => x.Id)
            .ToList();
        
        foreach (var relation in uniquePersons)
        {
            var reportPerson = new ReportPerson
            {
                Id = relation.Id,
                FirstName = relation.FirstName,
                LastName = relation.LastName,
                PersonalNumber = relation.PersonalNumber,
                ReportItems = relations.Where(r => r.NaturalPersonId == relation.Id || r.RelatedPersonId == relation.Id)
                    .GroupBy(x => x.Type)
                    .Select(x => new ReportItem
                    {
                        Type = x.Key,
                        Count = x.Count()
                    }).ToList()
            };
            reportPersons.Add(reportPerson);
        }
        
        var response = new GenerateReportResponse
        {
            Persons = reportPersons
        };
        
        return response;
    }

    public async Task CreateNaturalPersonAsync(CreateNaturalPersonRequest request)
    {
        var personExists =
            await _uof.NaturalPersonsRepository.GetFirstByFilterAsync(np => np.PersonalNumber == request.PersonalNumber) != null;
        if(personExists)
            throw new NaturalPersonExistsException(_localizer["Error.NaturalPerson.AlreadyExists"] + $" {request.PersonalNumber}");
        
        var personalNumberExists =
            await _uof.NaturalPersonsRepository.GetFirstByFilterAsync(pn => pn.PersonalNumber == request.PersonalNumber) != null;
        
        if(personalNumberExists)
            throw new PersonalNumberExists(_localizer["Error.PersonalNumber.AlreadyExists"] + $" {request.PersonalNumber}");
        
        var naturalPerson = new NaturalPerson
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PersonalNumber = request.PersonalNumber,
            BirthDate = request.BirthDate,
            CityId = request.CityId,
            Sex = request.Sex,
            PhoneNumbers = request.PhoneNumbers?.Select(pn => new PhoneNumber
            {
                Number = pn.Number,
                Type = pn.Type
            }).ToList() ?? []
        };
        
        await _uof.NaturalPersonsRepository.CreateAsync(naturalPerson);
        await _uof.SaveChangesAsync();
    }

    public async Task UpdateNaturalPersonAsync(int id, UpdateNaturalPersonRequest request)
    {
        var naturalPerson = await _uof.NaturalPersonsRepository.GetSingleByIdAsync(id);
        if (naturalPerson == null)
            throw new NotFoundException(_localizer["Error.NaturalPerson.NotFound"] + $" id: {id}");
        
        var personalNumberExists =
            await _uof.NaturalPersonsRepository.GetFirstByFilterAsync(pn => pn.PersonalNumber == request.PersonalNumber && pn.Id != id) != null;
        if(personalNumberExists)
            throw new PersonalNumberExists(_localizer["Error.PersonalNumber.AlreadyExists"] + $" {request.PersonalNumber}");
        
        naturalPerson.FirstName = request.FirstName;
        naturalPerson.LastName = request.LastName;
        naturalPerson.PersonalNumber = request.PersonalNumber;
        naturalPerson.BirthDate = request.BirthDate;
        naturalPerson.CityId = request.CityId;
        naturalPerson.PhoneNumbers = request.PhoneNumbers?.Select(pn => new PhoneNumber
        {
            Number = pn.Number,
            Type = pn.Type
        }).ToList() ?? new List<PhoneNumber>();
        
        await _uof.SaveChangesAsync();
    }

    public async Task UpdateNaturalPersonImageAsync(int id, UpdateNaturalPersonImageRequest request)
    {
        var naturalPerson = await _uof.NaturalPersonsRepository.GetSingleByIdAsync(id);
        if (naturalPerson == null)
            throw new NotFoundException(_localizer["Error.NaturalPerson.NotFound"] + $" id: {id}");

        var imagePath = await _fileService.UploadFileAsync(request.Image);
        naturalPerson.ImageUrl = imagePath;
        
        await _uof.SaveChangesAsync();
    }

    public async Task DeleteNaturalPersonAsync(int id)
    {
        var result = await _uof.NaturalPersonsRepository.DeleteSingleAsync(id);
        if (!result)
            throw new NotFoundException(_localizer["Error.NaturalPerson.NotFound"] + $" id: {id}");
        
        await _uof.SaveChangesAsync();
    }
}