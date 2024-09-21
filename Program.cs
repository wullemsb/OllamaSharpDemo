using OllamaSharp;
using OllamaSharp.Models.Chat;



var tool = new Tool()
{
    Function = new Function
    {
        Description = "Get the current weather for a location",
        Name = "GetWeather",
        Parameters = new Parameters
        {
            Properties = new Dictionary<string, Properties>
            {
                ["location"] = new()
                {
                    Type = "string",
                    Description = "The location to get the weather for, e.g. San Francisco, CA"
                },
                ["format"] = new()
                {
                    Type = "string",
                    Description = "The format to return the weather in, e.g. 'celsius' or 'fahrenheit'",
                    Enum = ["celsius", "fahrenheit"]
                },
            },
            Required = ["location", "format"],
        }
    },
    Type = "function"
};

var uri = new Uri("http://localhost:11434"); //Default Ollama endpoint
var ollama = new OllamaApiClient(uri);

ollama.SelectedModel = "llama3.1:latest";

string GetWeather(string location, string format)
{
    //Call the weather API here
    return $"The weather in {location} is 25 degrees {format}";
}


var chat = new Chat(ollama);
while (true)
{
    Console.Write("User>");
    var message = Console.ReadLine();
    Console.Write("Assistant>");
    await foreach (var answerToken in chat.Send(message, tools: [tool]))//We pass our tool when calling the LLM
        Console.Write(answerToken);

    //Check the latest message to see if a tool was called
    foreach (var toolCall in chat.Messages.Last().ToolCalls)
    {
        //var arguments = string.Join(",",toolCall.Function.Arguments.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        //Console.WriteLine($"Tool called:{toolCall.Function.Name} with following arguments: {arguments}");

        if (toolCall.Function.Name == nameof(GetWeather))
            Console.WriteLine(GetWeather(toolCall.Function.Arguments["location"], toolCall.Function.Arguments["format"]));
    }

    Console.WriteLine();
}
