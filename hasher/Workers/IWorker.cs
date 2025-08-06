namespace hasher.Workers
{
    public interface IWorker<Arg, Returns>
    {
        Task<Returns> DoWork(Arg arg);
    }
}
