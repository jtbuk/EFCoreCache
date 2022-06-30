namespace Jtbuk.EFCoreCache;
public record UpdateWeatherDto(int Temperature);
public record CreateWeatherDto(int Temperature);
public record GetWeatherDto(int Id, int Temperature, DateTime TimeStamp);