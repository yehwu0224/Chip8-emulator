namespace chip8wasm.Shared
{
    public class Chip8
    {
        // Chip-8 Specifications

        public byte[] Memory;       // Memory: 4K
        public byte[] Registers;    // Register: V0 ~ VF (bytes)
        public ushort I;            // I: 12 bits
        public ushort PC;           // Program Counter: 12 bits
        public Stack<ushort> Stack; // Stack: 12 bits

        public byte DelayTimer;
        public byte SoundTimer;

        public byte[] Keyboard;     // Keyboard: 16
        public byte[] Display;      // Display: 64 * 32

        public bool WaitingForKeyPress = false;


        public Chip8()
        {
            Memory = new byte[4096];
            Registers = new byte[16];
            I = 0;
            PC = 0;
            Stack = new Stack<ushort>(16);

            DelayTimer = 0;
            SoundTimer = 0;

            Keyboard = new byte[16];
            Display = new byte[64 * 32];
        }

        public void Reset()
        {
            Array.Clear(Memory, 0, Memory.Length);
            Array.Clear(Registers, 0, Registers.Length);
            Array.Clear(Display, 0, Display.Length);
            Stack.Clear();
            DelayTimer = 0;
            SoundTimer = 0;
            WaitingForKeyPress = false;
        }

        public void Keydown(byte key)
        {
            Keyboard[key] = 1;
            WaitingForKeyPress = false;
        }

        public void Keyup(byte key)
        {
            Keyboard[key] = 0;
        }

        private void InitializeFont()
        {
            var characters = new byte[]
            {
                    0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                    0x20, 0x60, 0x20, 0x20, 0x70, // 1
                    0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                    0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                    0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                    0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                    0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                    0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                    0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                    0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                    0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                    0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                    0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                    0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                    0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                    0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };

            Array.Copy(characters, Memory, characters.Length);
        }

        public void LoadProgram(byte[] program)
        {
            InitializeFont();
            for (int i = 0; i < program.Length; i++)
            {
                Memory[0x200 + i] = program[i];
            }
            PC = 0x200;
        }

        public void ExcuteOpcode()
        {
            if (DelayTimer > 0) DelayTimer--;
            if (SoundTimer > 0) SoundTimer--;

            if (WaitingForKeyPress)
            {
                return;
            }

            ushort opcode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            PC += 2;

            // var NNN = (ushort)(opcode & 0x0FFF);
            // var KK = (byte)(opcode & 0x00FF);
            // var N = (byte)(opcode & 0x000F);
            // var X = (byte)((opcode & 0x0F00) >> 8);
            // var Y = (byte)((opcode & 0x00F0) >> 4);

            ushort nipple = (ushort)(opcode & 0xF000);
            switch (nipple)
            {
                case 0x000:
                    if (opcode == 0x00E0)
                    {
                        for (int i = 0; i < Display.Length; i++) 
                        { 
                            Display[i] = 0; 
                        } 
                    }
                    else if (opcode == 0x00EE)
                        PC = Stack.Pop();
                    else
                        Console.WriteLine($"Invalid Opcode: {opcode.ToString("X4")}");
                    break;
                case 0x1000:
                    PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x2000:
                    Stack.Push(PC);
                    PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x3000:
                    if (Registers[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
                        PC += 2;
                    break;
                case 0x4000:
                    if (Registers[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
                        PC += 2;
                    break;
                case 0x5000:
                    if (Registers[(opcode & 0x0F00) >> 8] == Registers[(opcode & 0x00F0) >> 4])
                        PC += 2;
                    break;
                case 0x6000:
                    Registers[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
                    break;
                case 0x7000:
                    Registers[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF);
                    break;
                case 0x8000:
                    var vx = (opcode & 0x0F00) >> 8;
                    var vy = (opcode & 0x00F0) >> 4;
                    switch (opcode & 0x000F)
                    {
                        case 0: Registers[vx] = Registers[vy]; break;
                        case 1: Registers[vx] = (byte)(Registers[vx] | Registers[vy]); break;
                        case 2: Registers[vx] = (byte)(Registers[vx] & Registers[vy]); break;
                        case 3: Registers[vx] = (byte)(Registers[vx] ^ Registers[vy]); break;
                        case 4:
                            Registers[15] = (byte)((Registers[vx] + Registers[vy]) > 255 ? 1 : 0);
                            Registers[vx] = (byte)((Registers[vx] + Registers[vy]) & 0xFF);
                            break;
                        case 5:
                            Registers[15] = (byte)((Registers[vx] > Registers[vy]) ? 1 : 0);
                            Registers[vx] = (byte)((Registers[vx] - Registers[vy]) & 0xFF);
                            break;
                        case 6:
                            Registers[15] = (byte)(Registers[vx] & 0x000F);
                            Registers[vx] = (byte)(Registers[vx] >> 1);
                            break;
                        case 7:
                            Registers[15] = (byte)((Registers[vy] > Registers[vx]) ? 1 : 0);
                            Registers[vx] = (byte)((Registers[vy] - Registers[vx]) & 0xFF);
                            break;
                        case 0xE:
                            Registers[15] = (byte)(Registers[vx] >> 7);
                            Registers[vx] = (byte)(Registers[vx] << 1);
                            break;
                        default: Console.WriteLine($"Invalid Opcode: {opcode.ToString("X4")}"); break;
                    }
                    break;
                case 0x9000:
                    if (Registers[(opcode & 0x0F00) >> 8] != Registers[(opcode & 0x00F0) >> 4])
                        PC += 2;
                    break;
                case 0xA000:
                    I = (ushort)(opcode & 0x0FFF);
                    break;
                case 0xB000:
                    PC = (ushort)((opcode & 0x0FFF) + Registers[0]);
                    break;
                case 0xC000:
                    Registers[(opcode & 0x0F00) >> 8] = (byte)(new Random().Next(0, 255) & (opcode & 0x00FF));
                    break;
                case 0xD000:
                    var x = Registers[(opcode & 0x0F00) >> 8];
                    var y = Registers[(opcode & 0x00F0) >> 4];
                    var n = opcode & 0x000F; // sprite height

                    Registers[0xF] = 0; // Reset collision register to FALSE
                    for (int i = 0; i < n; i++)
                    {
                        byte mem = Memory[I + i];
                        for (int bit = 0; bit < 8; bit++)
                        {
                            byte pixel = (byte)((mem >> (7 - bit)) & 1);
                            var index = (x + bit) + ((y + i) * 64);

                            if (index > 2047) index -= 2048;

                            if (pixel == 1 && Display[index] != 0)
                            {
                                Registers[0xF] = 1; // Set collision register to TRUE
                            }
                            Display[index] = (byte)(Display[index] ^ pixel);
                        }
                    }
                    break;
                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x9E:
                            if (Keyboard[Registers[(opcode & 0x0F00) >> 8]] == 1)
                                PC += 2;
                            break;
                        case 0xA1:
                            if (Keyboard[Registers[(opcode & 0x0F00) >> 8]] == 0)
                                PC += 2;
                            break;
                        default: Console.WriteLine($"Invalid Opcode: {opcode.ToString("X4")}"); break;
                    }
                    break;
                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x07:
                            Registers[(opcode & 0x0F00) >> 8] = DelayTimer;
                            break;
                        case 0x0A:
                            WaitingForKeyPress = true;
                            PC -= 2;
                            break;
                        case 0x15:
                            DelayTimer = Registers[(opcode & 0x0F00) >> 8];
                            break;
                        case 0x18:
                            SoundTimer = Registers[(opcode & 0x0F00) >> 8];
                            break;
                        case 0x1E:
                            I += Registers[(opcode & 0x0F00) >> 8];
                            break;
                        case 0x29:
                            I = (ushort)(Registers[(opcode & 0x0F00) >> 8] * 5);
                            break;
                        case 0x33:
                            Memory[I] = (byte)(Registers[(opcode & 0x0F00) >> 8] / 100);
                            Memory[I + 1] = (byte)((Registers[(opcode & 0x0F00) >> 8] % 100) / 10);
                            Memory[I + 2] = (byte)(Registers[(opcode & 0x0F00) >> 8] % 10);
                            break;
                        case 0x55:
                            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); i++)
                            {
                                Memory[I + i] = Registers[i];
                            }
                            break;
                        case 0x65:
                            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); i++)
                            {
                                Registers[i] = Memory[I + i];
                            }
                            break;
                        default:
                            Console.WriteLine($"Invalid Opcode: {opcode.ToString("X4")}");
                            break;
                    }
                    break;

                default:
                    Console.WriteLine($"Invalid Opcode: {opcode.ToString("X4")}");
                    break;
            }
        }
    }
}
