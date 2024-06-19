namespace SampleApp
{
    using BasicIoC;

    public interface ISample
    {
        int Id
        {
            get;
        }

        string Name
        {
            get; set;
        }

        string DoSomething();
    }

    public class Program
    {
        public static IContainer MyContainer => Container.Instance;

        public static void Main()
        {
            string key = "Test";
            MyContainer.RegisterSingleton<ISample, Sample>();
            MyContainer.RegisterPrototype<ISample, Sample>(key);

            ISample? sample1 = MyContainer.ResolveDependency<ISample>();
            ArgumentNullException.ThrowIfNull(sample1);
            sample1.Name = "Alpha";
            ISample? sample2 = MyContainer.ResolveDependency<ISample>(key);
            ArgumentNullException.ThrowIfNull(sample2);
            sample2.Name = "Beta";
            ISample? sample3 = MyContainer.ResolveDependency<ISample>();
            ArgumentNullException.ThrowIfNull(sample3);
            sample3.Name = "Gamma";
            ISample? sample4 = MyContainer.ResolveDependency<ISample>(key);
            ArgumentNullException.ThrowIfNull(sample4);
            sample4.Name = "Delta";

            Console.WriteLine($"sample1 ID is {sample1.Id}");
            Console.WriteLine($"sample1.DoSomething() => {sample1.DoSomething()}");
            Console.WriteLine($"sample1 Name is {sample1.Name}");
            Console.WriteLine($"sample2 Name is {sample2.Name}");
            Console.WriteLine($"sample3 Name is {sample3.Name}");
            Console.WriteLine($"sample4 Name is {sample4.Name}");
        }
    }

    public class Sample : ISample
    {
        public int Id => 42;

        public string Name { get; set; } = string.Empty;

        public string DoSomething() => "Doing something...";
    }
}