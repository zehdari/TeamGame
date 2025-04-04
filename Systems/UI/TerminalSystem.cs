using ECS.Components.UI;
using ECS.Components.Tags;
using ECS.Core;
using ECS.Core.Utilities;
using ECS.Events;
using ECS.Components.State;

namespace ECS.Systems.UI
{
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
        
        // Store previous game state so we can restore it when closing the terminal.
        private GameState previousGameState;
        
        // The terminal should continue to work even when the game is paused
        public override bool Pausible => false;
        
        // Toggle key for the terminal
        private const Keys TOGGLE_KEY = Keys.OemTilde;

        public TerminalSystem(GameAssets assets, GraphicsManager graphicsManager)
        {
            this.graphicsManager = graphicsManager;
            this.spriteBatch = graphicsManager.spriteBatch;
            this.font = assets.GetFont("DebugFont"); // Assuming you have a debug font
            
            // Initialize key mappings. Note: you might not need every key if you only require alphanumerics.
            InitializeKeyMappings();
            
            // Initialize commands
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
                Console.WriteLine("Creating terminal entity");
                var terminal = world.CreateEntity();
                world.GetPool<TerminalTag>().Set(terminal, new TerminalTag());
                world.GetPool<TerminalComponent>().Set(terminal, new TerminalComponent
                {
                    IsActive = false,
                    CurrentInput = "",
                    History = new List<string>(),
                    HistoryIndex = -1,
                    OutputLines = new List<string> { "Terminal initialized. Type 'help' for a list of commands." },
                    ScrollPosition = 0,
                    MaxOutputLines = 100,
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
                    return "Available commands:\n" +
                           "  help - Show this help\n" +
                           "  clear - Clear terminal output\n" +
                           "  echo [text] - Echo text to terminal\n" +
                           "  exit - Close the terminal\n" +
                           "  entities - List all entities\n" +
                           "  fps [on|off] - Toggle FPS display";
                }},
                { "clear", args => {
                    foreach (var entity in World.GetEntities())
                    {
                        if (HasComponents<TerminalTag>(entity) && HasComponents<TerminalComponent>(entity))
                        {
                            ref var terminal = ref GetComponent<TerminalComponent>(entity);
                            terminal.OutputLines.Clear();
                            terminal.ScrollPosition = 0;
                            return "Terminal cleared.";
                        }
                    }
                    return "Terminal not found.";
                }},
                { "echo", args => {
                    if (args.Length > 0)
                    {
                        return string.Join(" ", args);
                    }
                    return "";
                }},
                { "exit", args => {
                    CloseTerminal();
                    return "Terminal closed.";
                }},
                { "entities", args => {
                    int count = World.GetEntities().Count();
                    return $"Total entities: {count}";
                }},
                { "fps", args => {
                    if (args.Length > 0)
                    {
                        bool enable = args[0].Equals("on", StringComparison.OrdinalIgnoreCase);
                        Publish(new ActionEvent
                        {
                            ActionName = "toggle_fps",
                            Entity = World.GetEntities().First(),
                            IsStarted = true,
                            IsEnded = false,
                            IsHeld = false
                        });
                        return $"FPS display {(enable ? "enabled" : "disabled")}.";
                    }
                    
                    Publish(new ActionEvent
                    {
                        ActionName = "toggle_fps",
                        Entity = World.GetEntities().First(),
                        IsStarted = true,
                        IsEnded = false,
                        IsHeld = false
                    });
                    return "FPS display toggled.";
                }}
            };
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
            return new Entity(0); // Return invalid entity if not found
        }

        private void AddOutputLine(ref TerminalComponent terminal, string text)
        {
            // Split text on newlines to handle multi-line output
            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                terminal.OutputLines.Add(line);
            }
            // Trim if we exceed max lines
            while (terminal.OutputLines.Count > terminal.MaxOutputLines)
            {
                terminal.OutputLines.RemoveAt(0);
            }
            // Auto-scroll to bottom to see new content
            terminal.ScrollPosition = 0;
        }

        private void ExecuteCommand(ref TerminalComponent terminal, string commandInput)
        {
            if (string.IsNullOrWhiteSpace(commandInput)) return;
            
            // Add command to history and output
            terminal.History.Add(commandInput);
            AddOutputLine(ref terminal, prompt + commandInput);
            
            // Parse command
            string[] parts = commandInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                string cmd = parts[0];
                string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();
                // Execute command if it exists
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

        private void ProcessKeyboardInput(ref TerminalComponent terminal)
        {
            // Handle key presses
            foreach (Keys key in Enum.GetValues<Keys>())
            {
                // Skip the toggle key to prevent it from being handled again
                if (key == TOGGLE_KEY) continue;
                
                if (currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key))
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
                            // Close terminal
                            CloseTerminal();
                            break;
                            
                        case Keys.Back:
                            // Backspace - remove last character
                            if (terminal.CurrentInput.Length > 0)
                            {
                                terminal.CurrentInput = terminal.CurrentInput.Substring(0, terminal.CurrentInput.Length - 1);
                            }
                            break;
                            
                        case Keys.Up:
                            // Navigate history upward
                            if (terminal.History.Count > 0 && terminal.HistoryIndex < terminal.History.Count - 1)
                            {
                                terminal.HistoryIndex++;
                                terminal.CurrentInput = terminal.History[terminal.History.Count - 1 - terminal.HistoryIndex];
                            }
                            break;
                            
                        case Keys.Down:
                            // Navigate history downward
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
                            // Scroll up (away from most recent text)
                            terminal.ScrollPosition = Math.Min(terminal.ScrollPosition + 5, Math.Max(0, terminal.OutputLines.Count - 1));
                            break;
                            
                        case Keys.PageDown:
                            // Scroll down (toward most recent text)
                            terminal.ScrollPosition = Math.Max(0, terminal.ScrollPosition - 5);
                            break;
                            
                        default:
                            // Add character to input if it's a printable key
                            if (keyMappings.TryGetValue(key, out var charMapping))
                            {
                                bool shift = currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift);
                                char c = shift ? charMapping.Shift : charMapping.Normal;
                                terminal.CurrentInput += c;
                            }
                            break;
                    }
                }
            }
        }
        
        private void OpenTerminal()
        {
            Entity terminalEntity = GetTerminalEntity();
            if (terminalEntity.Id == 0) return;
            
            // Store the current game state before switching to terminal mode
            previousGameState = GameStateHelper.GetGameState(World);
            ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);
            terminal.IsActive = true;
            
            // Set game state to Terminal
            GameStateHelper.SetGameState(World, GameState.Terminal);
            Console.WriteLine("Terminal opened, game state changed to Terminal");
        }
        
        private void CloseTerminal()
        {
            Entity terminalEntity = GetTerminalEntity();
            if (terminalEntity.Id == 0) return;
            
            ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);
            terminal.IsActive = false;
            
            // Restore the previous game state
            GameStateHelper.SetGameState(World, previousGameState);
            Console.WriteLine("Terminal closed, game state restored");
        }

        public override void Update(World world, GameTime gameTime)
        {
            // Update keyboard state
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            
            // Mouse Wheel Scrolling
            MouseState currentMouseState = Mouse.GetState();
            int scrollDelta = currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
            int scrollSteps = scrollDelta / 120; // assuming 120 per notch
            
            // Adjust scroll direction: add scrollSteps to scroll down (newer text) with a positive delta.
            if (scrollSteps != 0)
            {
                Entity terminalEntity = GetTerminalEntity();
                if (terminalEntity.Id != 0)
                {
                    ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);
                    terminal.ScrollPosition = Math.Clamp(terminal.ScrollPosition + (scrollSteps * 5), 0, Math.Max(0, terminal.OutputLines.Count - 1));
                }
            }
            previousMouseState = currentMouseState;
            
            // Update cursor blink timer
            cursorBlinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (cursorBlinkTimer >= 0.5f)
            {
                cursorBlinkTimer = 0f;
                showCursor = !showCursor;
            }
            
            // Get terminal entity
            Entity terminalEntityForUpdate = GetTerminalEntity();
            if (terminalEntityForUpdate.Id == 0) return;
            
            ref var terminalComponent = ref GetComponent<TerminalComponent>(terminalEntityForUpdate);
            
            // Check if state is Terminal to match component state
            bool isTerminalState = GameStateHelper.GetGameState(World) == GameState.Terminal;
            if (isTerminalState != terminalComponent.IsActive)
            {
                terminalComponent.IsActive = isTerminalState;
            }
            
            // Toggle terminal with tilde/grave key (direct keyboard check)
            if (currentKeyboardState.IsKeyDown(TOGGLE_KEY) && prevKeyboardState.IsKeyUp(TOGGLE_KEY))
            {
                Console.WriteLine($"Tilde pressed, terminal active: {terminalComponent.IsActive}");
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
            
            // Process keyboard input if terminal is active
            if (terminalComponent.IsActive)
            {
                ProcessKeyboardInput(ref terminalComponent);
            }
        }

        public void Draw(GameTime gameTime)
        {
            Entity terminalEntity = GetTerminalEntity();
            if (terminalEntity.Id == 0) return;
            
            ref var terminal = ref GetComponent<TerminalComponent>(terminalEntity);
            if (!terminal.IsActive) return;
            
            // Get viewport dimensions
            var viewport = graphicsManager.graphicsDevice.Viewport;
            int width = viewport.Width;
            int height = viewport.Height;
            
            // Draw background
            Texture2D pixel = new Texture2D(graphicsManager.graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            Rectangle backgroundRect = new Rectangle(0, 0, width, height / 2);
            spriteBatch.Draw(pixel, backgroundRect, backgroundColor);
            
            // Calculate visible area using scaled text
            int lineHeight = (int)(font.MeasureString("W").Y * textScale);
            int maxVisibleLines = (height / 2 - padding * 2) / lineHeight - 1; // -1 for input line
            int firstLineY = height / 2 - lineHeight - padding - lineHeight;
            
            // Determine the range of lines to display
            int startIndex = Math.Max(0, Math.Min(terminal.OutputLines.Count - 1 - terminal.ScrollPosition, terminal.OutputLines.Count - 1));
            int endIndex = Math.Max(0, startIndex - maxVisibleLines + 1);
            
            // Draw lines in reverse order (newest at bottom)
            for (int i = startIndex, lineIdx = 0; i >= endIndex; i--, lineIdx++)
            {
                string line = terminal.OutputLines[i];
                int y = firstLineY - (lineIdx * lineHeight);
                spriteBatch.DrawString(font, line, new Vector2(padding, y), textColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            }
            
            // Draw input line at the bottom of the terminal
            int inputY = height / 2 - lineHeight - padding;
            string inputText = prompt + terminal.CurrentInput;
            if (showCursor) inputText += "_";
            spriteBatch.DrawString(font, inputText, new Vector2(padding, inputY), promptColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            
            // Clean up the temporary texture
            pixel.Dispose();
        }
    }
}
