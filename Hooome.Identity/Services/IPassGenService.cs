namespace Hooome.Identity.Services;

public interface IPassGenService
{
    string Generate(int length = 12);
}
