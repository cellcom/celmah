namespace Celmah;

public interface IErrorFilter
{
    void OnErrorModuleFiltering(object sender, ExceptionFilterEventArgs args);
}