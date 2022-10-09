using Newtonsoft.Json.Linq;

public interface ISerializable // This is the interface that all the classes that need to be serialized need to implement
{
    string GetId(); // This is the method that returns the id of the object
    JObject Serialize(); // This method is used to serialize the class
    void Deserialize(JObject data); // This method is used to deserialize the class
}
