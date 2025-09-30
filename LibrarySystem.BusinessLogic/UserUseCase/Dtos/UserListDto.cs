namespace LibrarySystem.BusinessLogic.UserUseCase.Dtos;

public record class UserListDto
{

    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
