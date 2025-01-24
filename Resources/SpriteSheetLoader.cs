namespace ECSAttempt.Resources;

public static class SpriteSheetLoader
{
    // Private data classes just for parsing
    private class SpriteSheetJson
    {
        public Dictionary<string, StateJson> States { get; set; }
    }

    private class StateJson
    {
        public float Duration { get; set; }
        public List<FrameJson> Frames { get; set; }
    }

    private class FrameJson
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Duration { get; set; }
    }

    public static AnimationConfig LoadSpriteSheet(string jsonContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var data = JsonSerializer.Deserialize<SpriteSheetJson>(jsonContent, options);
        
        if (data?.States == null)
        {
            throw new InvalidOperationException("Invalid sprite sheet JSON: missing or null States");
        }

        var animConfig = new AnimationConfig
        {
            States = new Dictionary<string, AnimationFrameConfig[]>()
        };

        foreach (var (stateName, stateData) in data.States)
        {
            if (stateData?.Frames == null || stateData.Frames.Count == 0)
            {
                throw new InvalidOperationException($"Invalid state data for state: {stateName}");
            }

            var frames = new AnimationFrameConfig[stateData.Frames.Count];
            
            for (int i = 0; i < stateData.Frames.Count; i++)
            {
                var frame = stateData.Frames[i];
                frames[i] = new AnimationFrameConfig
                {
                    SourceRect = new Rectangle(frame.X, frame.Y, frame.Width, frame.Height),
                    Duration = frame.Duration > 0 ? frame.Duration : stateData.Duration
                };
            }
            
            animConfig.States[stateName] = frames;
        }

        return animConfig;
    }

    public static AnimationConfig LoadSpriteSheetFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Sprite sheet config not found at: {filePath}");
        }

        string jsonContent = File.ReadAllText(filePath);
        return LoadSpriteSheet(jsonContent);
    }
}