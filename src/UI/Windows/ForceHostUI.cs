// Dentro del método de renderizado de la UI (ej. OnGUI o DrawUtilitiesTab)

GUILayout.BeginHorizontal();

// Crea un botón con estilo de interruptor (On/Off)
string buttonText = ForceHost.IsEnabled ? "Force Host: [ACTIVADO]" : "Force Host: [DESACTIVADO]";

if (GUILayout.Button(buttonText, GUILayout.Width(200), GUILayout.Height(30)))
{
    // Cambia el estado del bot
    ForceHost.Toggle();
}

GUILayout.EndHorizontal();
