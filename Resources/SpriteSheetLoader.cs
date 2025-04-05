using ECS.Components.Animation;

namespace ECS.Resources;

public class SpriteSheetLoader : JsonLoaderBase<AnimationConfig>
{
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

    protected override AnimationConfig ParseJson(string jsonContent)
    {
        var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;

        var states = GetRequiredValue<Dictionary<string, StateJson>>(root, MAGIC.JSON_PARSING.STATES);
        
        var animConfig = new AnimationConfig
        {
            States = new Dictionary<string, AnimationFrameConfig[]>()
        };

        foreach (var (stateName, stateData) in states)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new InvalidOperationException("Invalid sprite sheet JSON: state name cannot be null or empty");
            }

            if (stateData?.Frames == null || stateData.Frames.Count == 0)
            {
                throw new InvalidOperationException($"Invalid state data for state: {stateName}");
            }

            var frames = new AnimationFrameConfig[stateData.Frames.Count];
            
            for (int i = 0; i < stateData.Frames.Count; i++)
            {
                var frame = stateData.Frames[i];
                if (frame.Width <= 0 || frame.Height <= 0)
                {
                    throw new InvalidOperationException($"Invalid frame dimensions in state: {stateName}, frame: {i}");
                }

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

    public static Rectangle GetSourceRect(AnimationConfig config, string stateName)
    {
        if (!config.States.ContainsKey(stateName))
        {
            throw new InvalidOperationException($"State '{stateName}' not found in sprite sheet configuration");
        }

        return config.States[stateName][0].SourceRect;
    }
}