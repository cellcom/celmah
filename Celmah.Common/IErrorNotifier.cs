using System.Threading.Tasks;

namespace Celmah;

public interface IErrorNotifier
{
    string Name { get; }

    Task NotifyAsync(Error error);
}