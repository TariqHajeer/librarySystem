using System;
using LibrarySystem.BusinessLogic.Domain.Attributes;

namespace LibrarySystem.BusinessLogic.Domain;


public class User
{
    [IgnoreOnInsert]
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    [IgnoreOnInsert]
    public DateTime CreatedAt { get; set; }
}