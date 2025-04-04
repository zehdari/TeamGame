using ECS.Core.Debug;
using ECS.Components.UI;
using ECS.Components.Tags;
using ECS.Core;
using ECS.Core.Utilities;
using ECS.Events;
using ECS.Components.State;
using ECS.Components;
using ECS.Components.Characters;

namespace ECS.Systems.Debug
{

    // If its debug it can't smell... right... right? 😅🤫

    // A struct to represent a segment of text with its own color
    public struct ColoredTextSegment
    {
        public string Text;
        public Color Color;

        public ColoredTextSegment(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }

    public class TerminalSystem : SystemBase
    {
        private readonly SpriteBatch spriteBatch;
        private readonly SpriteFont font;
        private readonly GraphicsManager graphicsManager;
        
        private KeyboardState currentKeyboardState;
        private KeyboardState prevKeyboardState;
        private MouseState previousMouseState;
        private float cursorBlinkTimer = 0f;
        private bool showCursor = true;
        private Dictionary<Keys, (char Normal, char Shift)> keyMappings;
        
        // Terminal appearance
        private readonly Color backgroundColor = new Color(0, 0, 0, 180);
        private readonly Color textColor = Color.White;
        private readonly Color promptColor = Color.White;
        private readonly int padding = 10;
        private readonly string prompt = "> ";
        private readonly float textScale = 0.9f;
        
        // Command dictionary for built-in commands
        private Dictionary<string, Func<string[], string>> commands;
        
        // Store previous game state so we can restore it when closing the terminal
        private GameState previousGameState;
        
        // The terminal should continue to work even when the game is paused
        public override bool Pausible => false;
        
        // Toggle key for the terminal
        private const Keys TOGGLE_KEY = Keys.OemTilde;

        // Constants for key repeat behavior (in seconds)
        private const float InitialDelay = 0.5f;
        private const float RepeatInterval = 0.1f;

        // Dictionary to track held keys and their elapsed time
        private Dictionary<Keys, float> heldKeyTimers = new Dictionary<Keys, float>();

        // Field to track logger messages processed so far
        private int lastLoggerMessageCount = 0;

        public TerminalSystem(GameAssets assets, GraphicsManager graphicsManager)
        {
            this.graphicsManager = graphicsManager;
            this.spriteBatch = graphicsManager.spriteBatch;
            this.font = assets.GetFont("DebugFont");
            
            InitializeKeyMappings();
            InitializeCommands();
        }

        public override void Initialize(World world)
        {
            base.Initialize(world);
            
            // Create terminal entity if it doesn't exist
            bool terminalExists = false;
            foreach (var entity in world.GetEntities())
            {
                if (HasComponents<TerminalTag>(entity) && HasComponents<TerminalComponent>(entity))
                {
                    terminalExists = true;
                    break;
                }
            }
            
            if (!terminalExists)
            {
                var terminal = world.CreateEntity();
                world.GetPool<TerminalTag>().Set(terminal, new TerminalTag());
                world.GetPool<TerminalComponent>().Set(terminal, new TerminalComponent
                {
                    IsActive = false,
                    CurrentInput = "",
                    History = new List<string>(),
                    HistoryIndex = -1,
                    OutputLines = new List<string> { "Type '<color=yellow>help</color>' for a list of commands." },
                    ScrollPosition = 0,
                    MaxOutputLines = 10000,
                    BackgroundOpacity = 0.8f
                });
                world.GetPool<SingletonTag>().Set(terminal, new SingletonTag());
            }
        }

        private void InitializeKeyMappings()
        {
            keyMappings = new Dictionary<Keys, (char Normal, char Shift)>
            {
                { Keys.A, ('a', 'A') },
                { Keys.B, ('b', 'B') },
                { Keys.C, ('c', 'C') },
                { Keys.D, ('d', 'D') },
                { Keys.E, ('e', 'E') },
                { Keys.F, ('f', 'F') },
                { Keys.G, ('g', 'G') },
                { Keys.H, ('h', 'H') },
                { Keys.I, ('i', 'I') },
                { Keys.J, ('j', 'J') },
                { Keys.K, ('k', 'K') },
                { Keys.L, ('l', 'L') },
                { Keys.M, ('m', 'M') },
                { Keys.N, ('n', 'N') },
                { Keys.O, ('o', 'O') },
                { Keys.P, ('p', 'P') },
                { Keys.Q, ('q', 'Q') },
                { Keys.R, ('r', 'R') },
                { Keys.S, ('s', 'S') },
                { Keys.T, ('t', 'T') },
                { Keys.U, ('u', 'U') },
                { Keys.V, ('v', 'V') },
                { Keys.W, ('w', 'W') },
                { Keys.X, ('x', 'X') },
                { Keys.Y, ('y', 'Y') },
                { Keys.Z, ('z', 'Z') },
                { Keys.D0, ('0', ')') },
                { Keys.D1, ('1', '!') },
                { Keys.D2, ('2', '@') },
                { Keys.D3, ('3', '#') },
                { Keys.D4, ('4', '$') },
                { Keys.D5, ('5', '%') },
                { Keys.D6, ('6', '^') },
                { Keys.D7, ('7', '&') },
                { Keys.D8, ('8', '*') },
                { Keys.D9, ('9', '(') },
                { Keys.OemMinus, ('-', '_') },
                { Keys.OemPlus, ('=', '+') },
                { Keys.OemOpenBrackets, ('[', '{') },
                { Keys.OemCloseBrackets, (']', '}') },
                { Keys.OemPipe, ('\\', '|') },
                { Keys.OemSemicolon, (';', ':') },
                { Keys.OemQuotes, ('\'', '"') },
                { Keys.OemComma, (',', '<') },
                { Keys.OemPeriod, ('.', '>') },
                { Keys.OemQuestion, ('/', '?') },
                { Keys.Space, (' ', ' ') },
                { Keys.NumPad0, ('0', '0') },
                { Keys.NumPad1, ('1', '1') },
                { Keys.NumPad2, ('2', '2') },
                { Keys.NumPad3, ('3', '3') },
                { Keys.NumPad4, ('4', '4') },
                { Keys.NumPad5, ('5', '5') },
                { Keys.NumPad6, ('6', '6') },
                { Keys.NumPad7, ('7', '7') },
                { Keys.NumPad8, ('8', '8') },
                { Keys.NumPad9, ('9', '9') },
            };
        }

        private void InitializeCommands()
        {
            commands = new Dictionary<string, Func<string[], string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "help", args => {
                    return "<color=yellow>Available commands</color>:\n" +
                        "  <color=plum>help</color> - Show this help\n" +
                        "  <color=plum>clear</color> - Clear terminal output\n" +
                        "  <color=plum>exit</color> - Close the terminal\n" +
                        "  <color=plum>entities</color> [-i] [-a] [-c] - List entities (-i: inspect, -a: include singletons, -c: only characters)\n" +
                        "  <color=plum>set</color> [entityId] [component] [property] [value] - Modify component value\n" +
                        "  <color=plum>fps</color> [on|off] - Toggle FPS display\n" +
                        "  <color=plum>movement</color> - Toggle movement vectors\n" +
                        "  <color=plum>hitboxes</color> - Toggle hitboxes\n" +
                        "  <color=plum>playerstate</color> - Toggle player state display\n" +
                        "  <color=plum>ids</color> - Toggle entity IDs display\n" +
                        "  <color=plum>mouse</color> - Toggle mouse coordinates display\n" +
                        "  <color=plum>polygon</color> - Toggle polygon creation mode\n" +
                        "  <color=plum>log</color> [view|clear|save [filename]] - Interact with the logger\n" +
                        "  <color=plum>inspect</color> [entityId] [-t] [Component] - Inspect components (optional type flag and component filter)\n" +
                        "  <color=plum>profile</color> [on|off] - Toggle system profiling";
                }},

                { "clear", args => {
                    foreach (var entity in World.GetEntities())
                    {
                        if (HasComponents<TerminalTag>(entity) && HasComponents<TerminalComponent>(entity))
                        {
                            ref var terminal = ref GetComponent<TerminalComponent>(entity);
                            terminal.OutputLines.Clear();
                            terminal.ScrollPosition = 0;
                            return "<color=yellow>Terminal cleared.</color>";
                        }
                    }
                    return "<color=lightcoral>Terminal not found? This is getting weird...</color>";
                }},
                { "exit", args => {
                    CloseTerminal();
                    return "<color=yellow>Terminal closed.</color>";
                }},
                { "entities", args => {
                    bool inspect = args.Contains("-i") || args.Contains("--inspect");
                    bool includeSingletons = args.Contains("-a") || args.Contains("--all");
                    bool filterCharacter = args.Contains("-c") || args.Contains("--character");

                    var allEntities = World.GetEntities();
                    var sb = new System.Text.StringBuilder();

                    int totalCount = 0;
                    int visibleCount = 0;
                    int singletonCount = 0;

                    foreach (var entity in allEntities)
                    {
                        totalCount++;
                        bool isSingleton = HasComponents<SingletonTag>(entity);
                        bool hasCharacter = HasComponents<CharacterConfig>(entity);

                        if (isSingleton && !includeSingletons)
                        {
                            singletonCount++;
                            continue;
                        }

                        if (filterCharacter && !hasCharacter)
                        {
                            continue;
                        }

                        visibleCount++;
                        var components = World.GetEntityComponents(entity);

                        if (inspect)
                        {
                            sb.AppendLine($"<color=yellow>Entity {entity.Id}:</color>");
                            foreach (var kv in components)
                            {
                                sb.AppendLine($"  <color=plum>{kv.Key.Name}</color>: {FormatValue(kv.Value)}");
                            }
                            if (components.Count == 0)
                            {
                                sb.AppendLine("  <color=lightcoral>No components attached.</color>");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"Entity {entity.Id}: {components.Count} components");
                        }
                    }

                    sb.Insert(0, $"<color=yellow>Total entities</color>: {totalCount} ({visibleCount} shown)\n");

                    if (singletonCount > 0 && !includeSingletons && visibleCount == 0)
                    {
                        sb.AppendLine($"\n<color=lightblue>{singletonCount} Entities with Singleton tag (use -a to include them)</color>");
                    }

                    return sb.ToString();
                }},
                { "fps", args => {
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_fps",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "<color=yellow>FPS display toggled.</color>";
                }},
                { "movement", args => {
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_movement_vectors",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "<color=yellow>Movement vectors toggled.</color>";
                }},
                { "hitboxes", args => {
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_hitboxes",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "<color=yellow>Hitboxes toggled.</color>";
                }},
                { "playerstate", args => {
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_player_state",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "<color=yellow>Player state toggled.</color>";
                }},
                { "ids", args => {
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_entity_ids",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "<color=yellow>Entity IDs toggled.</color>";
                }},
                { "mouse", args => {
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_mouse_coordinates",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "<color=yellow>Mouse coordinates toggled.</color>";
                }},
                { "polygon", args => {
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_polygon_creation",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "<color=yellow>Polygon creation toggled.</color>";
                }},
                { "log", args => {
                    if (args.Length == 0)
                    {
                        return "Usage: log [view|clear|save [filename]]";
                    }
                    string subCommand = args[0].ToLower();
                    switch (subCommand)
                    {
                        case "view":
                            return string.Join("\n", Logger.Messages);
                        case "clear":
                            Logger.Clear();
                            return "<color=yellow>Logger messages cleared.</color>";
                        case "save":
                            if (args.Length > 1)
                            {
                                return Logger.Save(args[1]);
                            }
                            else
                            {
                                return Logger.Save();
                            }
                        default:
                            return "<color=yellow>Unknown log command.</color> Use 'log view', 'log clear', or 'log save [filename]'.";
                    }
                }},
                { "inspect", args => {
                    if (args.Length == 0)
                    {
                        return "Usage: inspect [entityId] [-t|--types] [ComponentTypeName]";
                    }

                    bool showTypesOnly = args.Contains("-t") || args.Contains("--types");

                    string entityArg = args[0];
                    string filterComponent = null;

                    // Detect if a component type is passed as 2nd or 3rd arg
                    if (!showTypesOnly && args.Length >= 2)
                        filterComponent = args[1];
                    else if (showTypesOnly && args.Length >= 3)
                        filterComponent = args[2];

                    if (!int.TryParse(entityArg, out int entityId))
                    {
                        return "<color=lightcoral>Invalid entity ID. Please provide a numeric value.</color>";
                    }

                    Entity entity = World.GetEntityById(entityId);
                    if (entity.Id == 0)
                    {
                        return $"<color=lightcoral>Entity with ID {entityId} not found.</color>";
                    }

                    var output = new System.Text.StringBuilder();
                    output.AppendLine($"<color=yellow>Inspecting Entity</color>: {entityId}");

                    var components = World.GetEntityComponents(entity);
                    if (components.Count == 0)
                    {
                        output.AppendLine("<color=lightcoral>No components attached.</color>");
                    }
                    else
                    {
                        foreach (var kv in components)
                        {
                            if (!string.IsNullOrWhiteSpace(filterComponent) &&
                                !kv.Key.Name.Equals(filterComponent, StringComparison.OrdinalIgnoreCase) &&
                                !kv.Key.FullName.EndsWith(filterComponent, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            if (showTypesOnly)
                            {
                                output.AppendLine($"<color=plum>{kv.Key.FullName}</color>");
                            }
                            else
                            {
                                output.AppendLine($"<color=plum>{kv.Key.Name}</color>: {FormatValue(kv.Value)}");
                            }
                        }
                    }

                    return output.ToString();
                }},
                { "set", args => {
                    if (args.Length < 4)
                    {
                        return "Usage: set [entityId] [component] [property] [value]";
                    }
                    
                    if (!int.TryParse(args[0], out int entityId))
                    {
                        return "<color=lightcoral>Invalid entity ID. Please provide a numeric value.</color>";
                    }
                    
                    Entity entity = World.GetEntityById(entityId);
                    if (entity.Id == 0)
                    {
                        return $"<color=lightcoral>Entity with ID {entityId} not found.</color>";
                    }
                    
                    string componentName = args[1];
                    string propertyName = args[2];
                    string valueString = string.Join(" ", args.Skip(3));
                    
                    return SetComponentPropertyValue(entity, componentName, propertyName, valueString);
                }},
                { "profile", args => {
                    if (args.Length > 0)
                    {
                        bool enable = args[0].Equals("on", StringComparison.OrdinalIgnoreCase);
                        World.ProfilingEnabled = enable;
                        return $"<color=yellow>System profiling</color> {(enable ? "<color=lightgreen>enabled</color>" : "<color=lightcoral>disabled</color>")}.";
                    }
                    else
                    {
                        World.ProfilingEnabled = !World.ProfilingEnabled;
                        return $"<color=yellow>System profiling toggled to</color> {(World.ProfilingEnabled ? "<color=lightgreen>enabled</color>" : "<color=lightcoral>disabled</color>")}.";
                    }
                }},
            };
        }

        private string SetComponentPropertyValue(Entity entity, string componentName, string propertyName, string valueString)
        {
            // Get all the components for this entity
            var components = World.GetEntityComponents(entity);
            
            // Find the component type that matches the provided component name (case insensitive)
            Type matchingComponentType = null;
            object componentInstance = null;
            
            foreach (var pair in components)
            {
                if (pair.Key.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                {
                    matchingComponentType = pair.Key;
                    componentInstance = pair.Value;
                    break;
                }
            }
            
            if (matchingComponentType == null)
            {
                return $"<color=lightcoral>Component '{componentName}' not found on entity {entity.Id}.</color>";
            }
            
            // Find the property on the component
            PropertyInfo property = matchingComponentType.GetProperty(propertyName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            
            FieldInfo field = null;
            if (property == null)
            {
                // If property not found, try finding a field
                field = matchingComponentType.GetField(propertyName, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    
                if (field == null)
                {
                    return $"<color=lightcoral>Property or field '{propertyName}' not found on component '{componentName}'.</color>";
                }
            }
            
            try
            {
                // Convert the string value to the target type and set it
                object convertedValue;
                Type targetType = property != null ? property.PropertyType : field.FieldType;
                
                if (targetType == typeof(float))
                {
                    convertedValue = float.Parse(valueString, System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(int))
                {
                    convertedValue = int.Parse(valueString);
                }
                else if (targetType == typeof(bool))
                {
                    convertedValue = bool.Parse(valueString);
                }
                else if (targetType == typeof(string))
                {
                    convertedValue = valueString;
                }
                else if (targetType == typeof(Vector2))
                {
                    // Parse Vector2 from format like "10,20" or "10 20"
                    string[] parts = valueString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        return "<color=lightcoral>Invalid Vector2 format. Use 'x,y' or 'x y'.</color>";
                    }
                    float x = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                    convertedValue = new Vector2(x, y);
                }
                else if (targetType.IsEnum)
                {
                    convertedValue = Enum.Parse(targetType, valueString, true);
                }
                else
                {
                    return $"<color=lightcoral>Unsupported type: {targetType.Name}. Only primitive types, Vector2, and enums are supported.</color>";
                }
                
                // Create a mutable copy of the component (since structs are value types)
                object componentCopy = componentInstance;
                
                // Set the property/field value on the component
                if (property != null)
                {
                    property.SetValue(componentCopy, convertedValue);
                }
                else
                {
                    field.SetValue(componentCopy, convertedValue);
                }
                
                // Use reflection to set the modified component back to the entity
                var setMethod = GetSetMethodForComponent(matchingComponentType);
                if (setMethod != null)
                {
                    setMethod(World, entity, componentCopy);
                    return $"<color=lightgreen>Successfully set {componentName}.{propertyName} to {valueString} on entity {entity.Id}.</color>";
                }
                else
                {
                    return $"<color=lightcoral>Failed to update component: could not find appropriate Set method.</color>";
                }
            }
            catch (Exception ex)
            {
                return $"<color=lightcoral>Error setting value: {ex.Message}</color>";
            }
        }

        // Cache for component setter methods to avoid repeated reflection
        private static Dictionary<Type, Action<World, Entity, object>> componentSetterCache = new();

        private Action<World, Entity, object> GetSetMethodForComponent(Type componentType)
        {
            // Check cache first
            if (componentSetterCache.TryGetValue(componentType, out var setMethod))
            {
                return setMethod;
            }
            
            // Similar to the EntityUtils.CreateSetter method, create a delegate to set the component
            var worldParam = Expression.Parameter(typeof(World), "world");
            var entityParam = Expression.Parameter(typeof(Entity), "entity");
            var objParam = Expression.Parameter(typeof(object), "value");
            
            // Build: world.GetPool<T>()
            var getPoolMethod = typeof(World).GetMethod("GetPool").MakeGenericMethod(componentType);
            var getPoolCall = Expression.Call(worldParam, getPoolMethod);
            
            // Cast the value to the component type
            var castComponent = Expression.Convert(objParam, componentType);
            
            // Get the Set method on ComponentPool<T>
            var poolType = typeof(ComponentPool<>).MakeGenericType(componentType);
            var setMethod2 = poolType.GetMethod("Set");
            
            // Build: world.GetPool<T>().Set(entity, (T)value)
            var callSet = Expression.Call(getPoolCall, setMethod2, entityParam, castComponent);
            
            // Build the lambda
            var lambda = Expression.Lambda<Action<World, Entity, object>>(
                callSet,
                worldParam,
                entityParam,
                objParam
            );
            
            // Compile and cache the delegate
            var compiledDelegate = lambda.Compile();
            componentSetterCache[componentType] = compiledDelegate;
            
            return compiledDelegate;
        }
        
        private string FormatValue(object value)
        {
            if (value == null)
                return "null";

            Type type = value.GetType();

            if (type.Name == "Texture2D" || (type.FullName != null && type.FullName.Contains("Texture2D")))
            {
                var filenameProp = type.GetProperty("Filename");
                if (filenameProp != null)
                {
                    object filenameValue = filenameProp.GetValue(value);
                    return $"{filenameValue}";
                }
                var nameProp = type.GetProperty("Name");
                if (nameProp != null)
                {
                    object nameValue = nameProp.GetValue(value);
                    return $"{nameValue}";
                }
                return "Texture2D";
            }

            if (type.IsPrimitive || value is string || type.IsEnum)
                return value.ToString();

            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var items = new List<string>();
                foreach (var item in enumerable)
                {
                    items.Add(FormatValue(item));
                }
                return "[" + string.Join(", ", items) + "]";
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("{ ");
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {
                object propVal;
                try
                {
                    propVal = prop.GetValue(value);
                }
                catch
                {
                    propVal = "error";
                }
                sb.Append($"{prop.Name}: {FormatValue(propVal)}, ");
            }
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                object fieldVal;
                try
                {
                    fieldVal = field.GetValue(value);
                }
                catch
                {
                    fieldVal = "error";
                }
                sb.Append($"{field.Name}: {FormatValue(fieldVal)}, ");
            }
            if (sb.Length > 2)
                sb.Length -= 2;
            sb.Append(" }");
            return sb.ToString();
        }

        private Entity GetTerminalEntity()
        {
            foreach (var entity in World.GetEntities())
            {
                if (HasComponents<TerminalTag>(entity) && HasComponents<TerminalComponent>(entity))
                {
                    return entity;
                }
            }
            return new Entity(0);
        }

        // Adds output to the terminal.
        private void AddOutputLine(ref TerminalComponent terminal, string text)
        {
            if (text == null) return;

            text = text.Replace("\r\n", "\n").Replace("\t", "    "); // Replace tab with 4 spaces, or adjust as needed
            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                terminal.OutputLines.Add(line);
            }

            while (terminal.OutputLines.Count > terminal.MaxOutputLines)
            {
                terminal.OutputLines.RemoveAt(0);
            }

            terminal.ScrollPosition = 0;
        }

        private void ExecuteCommand(ref TerminalComponent terminal, string commandInput)
        {
            if (string.IsNullOrWhiteSpace(commandInput))
                return;
            
            terminal.History.Add(commandInput);
            AddOutputLine(ref terminal, prompt + commandInput);
            
            string[] parts = commandInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                string cmd = parts[0];
                string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();
                if (commands.TryGetValue(cmd, out var handler))
                {
                    string output = handler(args);
                    if (!string.IsNullOrEmpty(output))
                    {
                        AddOutputLine(ref terminal, output);
                    }
                }
                else
                {
                    AddOutputLine(ref terminal, $"Unknown command: {cmd}");
                }
            }
        }

        private void ProcessKeyboardInput(ref TerminalComponent terminal, GameTime gameTime)
        {
            foreach (Keys key in Enum.GetValues<Keys>())
            {
                if (key == TOGGLE_KEY)
                    continue;

                bool isDown = currentKeyboardState.IsKeyDown(key);
                bool wasDown = prevKeyboardState.IsKeyDown(key);

                if (isDown && !wasDown)
                {
                    ProcessKey(key, ref terminal);
                    heldKeyTimers[key] = 0f;
                }
                else if (isDown && wasDown)
                {
                    if (heldKeyTimers.ContainsKey(key))
                    {
                        heldKeyTimers[key] += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (heldKeyTimers[key] >= InitialDelay)
                        {
                            while (heldKeyTimers[key] >= InitialDelay)
                            {
                                ProcessKey(key, ref terminal);
                                heldKeyTimers[key] -= RepeatInterval;
                            }
                        }
                    }
                }
                else if (!isDown && wasDown)
                {
                    heldKeyTimers.Remove(key);
                }
            }
        }

        private void ProcessKey(Keys key, ref TerminalComponent terminal)
        {
            switch (key)
            {
                case Keys.Enter:
                    if (!string.IsNullOrWhiteSpace(terminal.CurrentInput))
                    {
                        string input = terminal.CurrentInput;
                        terminal.CurrentInput = "";
                        terminal.HistoryIndex = -1;
                        ExecuteCommand(ref terminal, input);
                    }
                    break;
                case Keys.Escape:
                    CloseTerminal();
                    break;
                case Keys.Back:
                    if (terminal.CurrentInput.Length > 0)
                    {
                        terminal.CurrentInput = terminal.CurrentInput.Substring(0, terminal.CurrentInput.Length - 1);
                    }
                    break;
                case Keys.Up:
                    if (terminal.History.Count > 0 && terminal.HistoryIndex < terminal.History.Count - 1)
                    {
                        terminal.HistoryIndex++;
                        terminal.CurrentInput = terminal.History[terminal.History.Count - 1 - terminal.HistoryIndex];
                    }
                    break;
                case Keys.Down:
                    if (terminal.HistoryIndex > 0)
                    {
                        terminal.HistoryIndex--;
                        terminal.CurrentInput = terminal.History[terminal.History.Count - 1 - terminal.HistoryIndex];
                    }
                    else if (terminal.HistoryIndex == 0)
                    {
                        terminal.HistoryIndex = -1;
                        terminal.CurrentInput = "";
                    }
                    break;
                case Keys.PageUp:
                    terminal.ScrollPosition = Math.Min(terminal.ScrollPosition + 5, int.MaxValue);
                    break;
                case Keys.PageDown:
                    terminal.ScrollPosition = Math.Max(terminal.ScrollPosition - 5, 0);
                    break;
                default:
                    if (keyMappings.TryGetValue(key, out var charMapping))
                    {
                        bool shift = currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift);
                        char c = shift ? charMapping.Shift : charMapping.Normal;
                        terminal.CurrentInput += c;
                    }
                    break;
            }
        }

        private void OpenTerminal()
        {
            Entity terminalEntity = GetTerminalEntity();
            if (terminalEntity.Id == 0)
                return;
            
            previousGameState = GameStateHelper.GetGameState(World);
            ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);

            foreach (var message in Logger.Messages)
            {
                AddOutputLine(ref terminal, message);
            }
            lastLoggerMessageCount = Logger.Messages.Count;
            
            terminal.IsActive = true;
            GameStateHelper.SetGameState(World, GameState.Terminal);
        }

        private void CloseTerminal()
        {
            Entity terminalEntity = GetTerminalEntity();
            if (terminalEntity.Id == 0)
                return;
            
            ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);
            terminal.IsActive = false;
            GameStateHelper.SetGameState(World, previousGameState);
        }

        public override void Update(World world, GameTime gameTime)
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            
            MouseState currentMouseState = Mouse.GetState();
            int scrollDelta = currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
            int scrollSteps = scrollDelta / 120;
            
            if (scrollSteps != 0)
            {
                Entity terminalEntity = GetTerminalEntity();
                if (terminalEntity.Id != 0)
                {
                    ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);
                    var viewport = graphicsManager.graphicsDevice.Viewport;
                    int width = viewport.Width;
                    float maxTextWidth = width - 2 * padding;
                    int totalWrappedLines = 0;
                    // Count wrapped lines for all output lines
                    foreach (var line in terminal.OutputLines)
                    {
                        var segments = ParseColoredText(line);
                        var wrapped = WrapColoredText(segments, font, maxTextWidth, textScale);
                        totalWrappedLines += wrapped.Count;
                    }
                    terminal.ScrollPosition = Math.Clamp(terminal.ScrollPosition + (scrollSteps * 5), 0, Math.Max(0, totalWrappedLines - 1));
                }
            }
            previousMouseState = currentMouseState;
            
            cursorBlinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (cursorBlinkTimer >= 0.5f)
            {
                cursorBlinkTimer = 0f;
                showCursor = !showCursor;
            }
            
            Entity terminalEntityForUpdate = GetTerminalEntity();
            if (terminalEntityForUpdate.Id == 0)
                return;
            
            ref var terminalComponent = ref GetComponent<TerminalComponent>(terminalEntityForUpdate);
            
            bool isTerminalState = GameStateHelper.GetGameState(World) == GameState.Terminal;
            if (isTerminalState != terminalComponent.IsActive)
            {
                terminalComponent.IsActive = isTerminalState;
            }
            
            if (currentKeyboardState.IsKeyDown(TOGGLE_KEY) && prevKeyboardState.IsKeyUp(TOGGLE_KEY))
            {
                if (terminalComponent.IsActive)
                {
                    CloseTerminal();
                }
                else
                {
                    OpenTerminal();
                }
                return;
            }
            
            if (terminalComponent.IsActive)
            {
                ProcessKeyboardInput(ref terminalComponent, gameTime);

                int currentLogCount = Logger.Messages.Count;
                if (currentLogCount > lastLoggerMessageCount)
                {
                    for (int i = lastLoggerMessageCount; i < currentLogCount; i++)
                    {
                        AddOutputLine(ref terminalComponent, Logger.Messages[i]);
                    }
                    lastLoggerMessageCount = currentLogCount;
                }
            }
        }

        // Parses a string for simple color markup tags
        // Example: <color=red>This text is red</color>
        private List<ColoredTextSegment> ParseColoredText(string text)
        {

            var segments = new List<ColoredTextSegment>();
            Color currentColor = textColor;
            int pos = 0;

            if (string.IsNullOrEmpty(text))
            {
                segments.Add(new ColoredTextSegment("", textColor));
                return segments;
            }
            

            while (pos < text.Length)
            {
                int tagStart = text.IndexOf("<color=", pos, StringComparison.OrdinalIgnoreCase);
                if (tagStart == -1)
                {
                    segments.Add(new ColoredTextSegment(text.Substring(pos), currentColor));
                    break;
                }
                if (tagStart > pos)
                {
                    segments.Add(new ColoredTextSegment(text.Substring(pos, tagStart - pos), currentColor));
                }
                int tagEnd = text.IndexOf(">", tagStart);
                if (tagEnd == -1)
                {
                    segments.Add(new ColoredTextSegment(text.Substring(tagStart), currentColor));
                    break;
                }
                string colorName = text.Substring(tagStart + 7, tagEnd - (tagStart + 7));
                currentColor = colorName.ToLower() switch
                {
                    "red" => Color.Red,
                    "green" => Color.Green,
                    "blue" => Color.Blue,
                    "yellow" => Color.Yellow,
                    "orange" => Color.Orange,
                    "purple" => Color.Purple,
                    "plum" => Color.Plum,
                    "lightgreen"  => Color.LightGreen,
                    "lightblue"   => Color.LightBlue,
                    "lightcoral"  => Color.LightCoral,
                    _ => textColor,
                };
                pos = tagEnd + 1;
                int closeTag = text.IndexOf("</color>", pos, StringComparison.OrdinalIgnoreCase);
                if (closeTag == -1)
                {
                    segments.Add(new ColoredTextSegment(text.Substring(pos), currentColor));
                    break;
                }
                else
                {
                    string coloredPart = text.Substring(pos, closeTag - pos);
                    segments.Add(new ColoredTextSegment(coloredPart, currentColor));
                    pos = closeTag + 8;
                    currentColor = textColor;
                }
            }
            return segments;
        }

        // Wraps a list of colored text segments into multiple lines based on maxWidth
        private List<List<ColoredTextSegment>> WrapColoredText(List<ColoredTextSegment> segments, SpriteFont font, float maxWidth, float scale)
        {
            var lines = new List<List<ColoredTextSegment>>();
            var currentLine = new List<ColoredTextSegment>();
            float currentWidth = 0f;

            foreach (var segment in segments)
            {
                // Split segment text into words while preserving spaces
                string[] words = segment.Text.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    // Append a space if not the last word
                    string wordWithSpace = i < words.Length - 1 ? words[i] + " " : words[i];
                    float wordWidth = font.MeasureString(wordWithSpace).X * scale;

                    if (currentWidth + wordWidth > maxWidth && currentLine.Count > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = new List<ColoredTextSegment>();
                        currentWidth = 0f;
                    }

                    // If the word itself is too long, split it into characters
                    if (wordWidth > maxWidth)
                    {
                        foreach (char c in wordWithSpace)
                        {
                            string s = c.ToString();
                            float charWidth = font.MeasureString(s).X * scale;
                            if (currentWidth + charWidth > maxWidth && currentLine.Count > 0)
                            {
                                lines.Add(currentLine);
                                currentLine = new List<ColoredTextSegment>();
                                currentWidth = 0f;
                            }
                            currentLine.Add(new ColoredTextSegment(s, segment.Color));
                            currentWidth += charWidth;
                        }
                    }
                    else
                    {
                        currentLine.Add(new ColoredTextSegment(wordWithSpace, segment.Color));
                        currentWidth += wordWidth;
                    }
                }
            }

            if (currentLine.Count > 0)
                lines.Add(currentLine);

            return lines;
        }

        public void Draw(GameTime gameTime)
        {
            Entity terminalEntity = GetTerminalEntity();
            if (terminalEntity.Id == 0)
                return;
            
            ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);
            if (!terminal.IsActive)
                return;
            
            var viewport = graphicsManager.graphicsDevice.Viewport;
            int width = viewport.Width;
            int height = viewport.Height;
            
            // Draw terminal background (occupies the top half)
            Texture2D pixel = new Texture2D(graphicsManager.graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            Rectangle backgroundRect = new Rectangle(0, 0, width, height / 2);
            spriteBatch.Draw(pixel, backgroundRect, backgroundColor);
            
            int lineHeight = (int)(font.MeasureString("W").Y * textScale);
            int inputY = backgroundRect.Bottom - padding - lineHeight;
            float maxTextWidth = width - 2 * padding;

            // Build a list of all wrapped lines from the terminal output
            var allWrappedLines = new List<List<ColoredTextSegment>>();
            foreach (var outputLine in terminal.OutputLines)
            {
                var segments = ParseColoredText(outputLine);
                var wrapped = WrapColoredText(segments, font, maxTextWidth, textScale);
                allWrappedLines.AddRange(wrapped);
            }

            int totalWrappedLines = allWrappedLines.Count;
            int maxVisibleLines = (inputY - (backgroundRect.Top + padding)) / lineHeight;
            int startIndex = Math.Max(0, totalWrappedLines - 1 - terminal.ScrollPosition);
            int endIndex = Math.Max(0, startIndex - maxVisibleLines + 1);
            int drawLine = 0;
            for (int i = startIndex; i >= endIndex; i--, drawLine++)
            {
                var lineSegments = allWrappedLines[i];
                float x = padding;
                int y = inputY - lineHeight - (drawLine * lineHeight);
                foreach (var segment in lineSegments)
                {
                    spriteBatch.DrawString(font, segment.Text, new Vector2(x, y), segment.Color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                    x += font.MeasureString(segment.Text).X * textScale;
                }
            }
            
            string inputText = prompt + terminal.CurrentInput;
            if (showCursor)
                inputText += "_";
            spriteBatch.DrawString(font, inputText, new Vector2(padding, inputY), promptColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            
            pixel.Dispose();
        }
    }
}