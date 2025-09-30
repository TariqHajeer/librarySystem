using System;

namespace LibrarySystem.DataAccess.Exceptions;

public class DuplicateRecordException : ConflictException
{
    public DuplicateRecordException(string message) : base(message) { }
}