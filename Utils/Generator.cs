namespace App.Utils;

public interface IGenerator
{
    Task<string> GenerateNanoId(int size);
}

public class Generator : IGenerator
{
    public async Task<string> GenerateNanoId(int size)
    {
        return await NanoidDotNet.Nanoid.GenerateAsync(
            size: size
        );
    }
}

