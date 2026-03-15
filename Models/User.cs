
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

public class User
{
    [Required]
    public string Name {get;set;}
    [Required]
    public string Email{get;set;}
    [Required]
    public string PasswordHash {get;set;}
}