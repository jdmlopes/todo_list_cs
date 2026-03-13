using System.ComponentModel.DataAnnotations;

namespace TodoApi.Utils;

public static class ValidationHelper
{
    public static List<ValidationResult>? Validate(object model)
    {
        ValidationContext validationContext = new ValidationContext(model);
        List<ValidationResult> errors = new();
        if (!Validator.TryValidateObject(model, validationContext, errors, true))
        {
            return errors;
        }
        return null;
    }
}