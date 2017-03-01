using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework.Math;
using ICities;
using RealisticWinds.Redirection;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace RealisticWinds
{
    [TargetType(typeof(WeatherManager))]
    public class WindSpeedExtension : ThreadingExtensionBase
    {
        private static uint calmProbability = 25u;
        private static int minCalmIntensify = 2500;
        private static int maxCalmIntensify = 10000; //don't change this value
        private static uint noCalmCounter = 0u;
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);

        private static float currentCalm;
        private static float targetCalm;
        private static FogProperties fogProperties;
        private static float windSpeedBase;
        private static DayNightDynamicCloudsProperties dynamicCloudsProperties;
        private static float windForceBase;

        private static Dictionary<MethodInfo, RedirectCallsState> _redirects;
        private static bool initialized;

        public static void Deploy()
        {
            if (_redirects != null)
            {
                return;
            }
            _redirects = RedirectionUtil.RedirectType(typeof(WindSpeedExtension));
            fogProperties = Object.FindObjectOfType<FogProperties>();
            windSpeedBase = fogProperties.m_WindSpeed;
            dynamicCloudsProperties = Object.FindObjectOfType<DayNightDynamicCloudsProperties>();
            windForceBase = dynamicCloudsProperties.m_WindForce;
            targetCalm = 0;
            currentCalm = 0;
            noCalmCounter = 0;
            initialized = true;
        }

        public static void Revert()
        {
            if (_redirects == null)
            {
                return;
            }
            foreach (var redirect in _redirects)
            {
                RedirectionHelper.RevertRedirect(redirect.Key, redirect.Value);
            }
            _redirects = null;
            noCalmCounter = 0;
            targetCalm = 0;
            currentCalm = 0;
            initialized = false;
        }


        public override void OnAfterSimulationTick()
        {
            if (!initialized)
            {
                return;
            }
            if (noCalmCounter > 0)
            {
                noCalmCounter--;
                return;
            }
            if (targetCalm > currentCalm)
            {
                currentCalm = Mathf.Min(targetCalm, currentCalm + 0.0002f);
            }
            else if (targetCalm < currentCalm)
            {
                currentCalm = Mathf.Max(targetCalm, currentCalm - 0.0002f);
            }
            else
            {
                if (random.Next(100) < calmProbability)
                {
                    targetCalm = (float)random.Next(minCalmIntensify, maxCalmIntensify) * 0.0001f;
                }
                else
                {
                    targetCalm = 0f;
                    if (Math.Abs(currentCalm) < 0.0001)
                    {
                        noCalmCounter = 5000;
                    }
                }
                //UnityEngine.Debug.Log($"Target calm intensity: {targetCalm}");
            }
            fogProperties.m_WindSpeed = windSpeedBase * GetWindSpeedFactor();
            dynamicCloudsProperties.m_WindForce = windForceBase * GetWindSpeedFactor();
        }

        [RedirectMethod]
        private float GetWindSpeedFactor()
        {
            return (float)(1.0 + WeatherManager.instance.m_currentRain * 0.5 - WeatherManager.instance.m_currentFog * 0.5) * (1.0f - currentCalm);
        }
    }
}
