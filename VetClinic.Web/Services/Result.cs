namespace VetClinic.Web.Services;

// İş katmanı operasyon sonucu. View/HTTP bilmez; Controller bunu okuyup ModelState/TempData'ya çevirir.
public class Result
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = "";

    public static Result Success(string msg = "") => new() { Succeeded = true, Message = msg };
    public static Result Fail(string msg) => new() { Succeeded = false, Message = msg };
}
