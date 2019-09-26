namespace multiple_indexer
{
    public class Animal
    {
        public string Name { get; set; }

        public bool IsDangerous { get; set; }
    }

    public class Fish : Animal
    {
        public double Salinity { get; set; }
    }

    public class Bird : Animal
    {
        public bool CanFly { get; set; }

        public int Wingspan { get; set; }
    }
}
