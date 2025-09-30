using System;
using LibrarySystem.BusinessLogic.Domain.Attributes;

namespace LibrarySystem.BusinessLogic.Domain;

public class Book
{
    [IgnoreOnInsert]
    public Guid Id { get; set; } 
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public int PublishedYear { get; set; }
    public int TotalCopies { get; set; } = 1;
    public int AvailableCopies { get; set; } = 1;
}