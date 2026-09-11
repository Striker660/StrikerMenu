using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using InnerNet;
using UnityEngine;

namespace HyperMenu.Modules
{
    public static class ForceHost
    {
        public static bool IsEnabled = false;
        private static Coroutine activeCoroutine = null;

        // Método para encender/apagar desde la UI
        public static void Toggle()
        {
            IsEnabled = !IsEnabled;

            if (IsEnabled)
            {
                activeCoroutine = HyperMenuPlugin.Instance.StartCoroutine(ForceHostLoop());
            }
            else
            {
                if (activeCoroutine != null)
                {
                    HyperMenuPlugin.Instance.StopCoroutine(activeCoroutine);
                    activeCoroutine = null;
                }
            }
        }

        private static IEnumerator ForceHostLoop()
        {
            while (IsEnabled)
            {
                // 1. Verificar si estamos dentro de una sala/lobby
                if (AmongUsClient.Instance != null && AmongUsClient.Instance.InOnlineScene)
                {
                    // Si ya somos el Host, detenemos el proceso
                    if (AmongUsClient.Instance.AmHost)
                    {
                        IsEnabled = false;
                        yield break;
                    }

                    // 2. Intentar Votekick/Expulsar al Host actual mediante el RPC de Kick
                    PlayerControl hostPlayer = GetHostPlayerControl();
                    if (hostPlayer != null)
                    {
                        // Enviar petición de Kick para intentar tumbar/votar al Host actual
                        AmongUsClient.Instance.KickPlayer(hostPlayer.Client, false);
                    }

                    yield return new WaitForSeconds(0.3f);

                    // 3. Guardar el código del lobby actual para reconectarse
                    string gameCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);

                    // 4. Salir de la partida (Disconnect)
                    AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);

                    // Esperar a volver al menú principal
                    yield return new WaitForSeconds(1.0f);

                    // 5. Reconectarse a la misma sala usando el código
                    if (!string.IsNullOrEmpty(gameCode))
                    {
                        int gameId = GameCode.GameNameToInt(gameCode);
                        AmongUsClient.Instance.Connect(MatchMakerModes.Client);
                        AmongUsClient.Instance.JoinGame(gameId);
                    }

                    // Esperar a entrar a la partida antes del siguiente ciclo
                    yield return new WaitForSeconds(2.5f);
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                }
            }
        }

        private static PlayerControl GetHostPlayerControl()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player != null && player.Data != null && player.OwnerId == AmongUsClient.Instance.HostId)
                {
                    return player;
                }
            }
            return null;
        }
    }
}
