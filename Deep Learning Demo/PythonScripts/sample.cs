using System;
using Python.Runtime;

class csSample
{
    static void Sample()
    {
        // Set Python Home to the root of your virtual environment
        PythonEngine.PythonHome = @"C:\path\to\your\venv";

        // Set Python Path to include standard library and site-packages inside venv
        PythonEngine.PythonPath = string.Join(";",
            @"C:\path\to\your\venv\Lib",
            @"C:\path\to\your\venv\Lib\site-packages"
        );

        // Initialize the Python engine
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            dynamic sys = Py.Import("sys");
            Console.WriteLine("Python version: " + sys.version);

            // Append the directory containing model_runner.py
            sys.path.append(@"C:\path\to\your\python\modules");

            // Now you can import your Python module
            dynamic model_module = Py.Import("model_runner");

            // Instantiate your class (assuming ModelRunner exists in model_runner.py)
            dynamic runner = model_module.ModelRunner();

            // Use your runner here...
        }

        // Shutdown the Python engine when done
        PythonEngine.Shutdown();
    }
}
