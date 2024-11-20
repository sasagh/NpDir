namespace NpDirectory.Application.Exceptions;

public class PersonalNumberExistsException(string message) : Exception(message);