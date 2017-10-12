namespace Exercise.Domain.Services
{
    public interface IJsonService
    {
        T ParseJson<T>(string jsonText);

        Meeting Transform(RaceData raceData);

        RaceData ParseOriginalSample();
    }
}