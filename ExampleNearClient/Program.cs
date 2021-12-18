

namespace ExampleNearClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Example example = new Example();
            example.Init();
            example.WaitCommand().Wait();
        }
    }
}
