namespace Jam.Models;

public class AnswerOption
{
    public int AnswerOptionId { get; set; }
    public string CorrectText { get; set; } = string.Empty;
    public string WrongText1 { get; set; } = string.Empty;
    public string WrongText2 { get; set; } = string.Empty;
    public string WrongText3 { get; set; } = string.Empty;
    public bool QuestionID { get; set; }
    public Question? Question { get; set; } // navigation property 
}