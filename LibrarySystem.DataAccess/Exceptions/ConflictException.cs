using System;

namespace LibrarySystem.DataAccess.Exceptions;

public class ConflictException(string message) : Exception(message)
{
}
