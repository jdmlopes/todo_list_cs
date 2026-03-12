using System.ComponentModel.DataAnnotations;

public class Todo
{
    public int Id {get; set;}
    [Required]
    public string Title {get;set;}
    [Required]
    public string Description {get; set;}
    public bool Completed {get; set;}

}