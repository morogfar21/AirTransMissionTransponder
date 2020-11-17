namespace ATM.System
{
    public interface IFlightCalculator
    {
        double CalculateSpeed(TransponderData oldData, TransponderData newData);
        double CalculateDirection(TransponderData oldData, TransponderData newData);
    }
}