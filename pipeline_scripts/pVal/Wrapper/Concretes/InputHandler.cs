namespace Wrapper
{
    using System;

    public class InputHandler : IInputHandler
    {
        internal InputHandler()
        {
        }

        public string GetUserInput()
        {
            return Console.ReadLine();
        }
    }
}
