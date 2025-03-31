namespace ECS.Resources;

public interface IJsonLoader<T>
{
    T LoadFromJson(string jsonContent);
    T LoadFromFile(string filePath);
}