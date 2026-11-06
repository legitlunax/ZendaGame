using System;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisDemo
    {
        private string discoverySearch = "";
        private bool availableDiscoveriesOnly;

        public void SetDiscoveryFilter(string query, bool availableOnly)
        {
            discoverySearch = (query ?? "").Trim();
            availableDiscoveriesOnly = availableOnly;
            scroll = Vector2.zero;
        }

        private bool MatchesDiscovery(GeneratorDefinition generator)
        {
            // Locked names remain concealed until the normal discovery gate opens.
            bool discovered = Game.IsDiscovered(generator.id);
            if (availableDiscoveriesOnly && !discovered) return false;
            if (discoverySearch.Length == 0) return true;
            if (!discovered) return false;
            return Contains(generator.name, discoverySearch) || Contains(generator.fairy, discoverySearch) ||
                Contains(generator.era, discoverySearch);
        }

        private static bool Contains(string value, string query)
        {
            return value != null && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public int VisibleDiscoveryCount
        {
            get
            {
                int count = 0;
                if (Game == null) return count;
                foreach (GeneratorDefinition generator in catalog.generators)
                    if (MatchesDiscovery(generator)) count++;
                return count;
            }
        }

        public void FocusNextDiscovery()
        {
            SetDiscoveryFilter("", true);
            tab = 0;
            int index = 0;
            foreach (GeneratorDefinition generator in catalog.generators)
            {
                if (!MatchesDiscovery(generator)) continue;
                if (Game.Counts[generator.id] == 0) break;
                index++;
            }
            scroll.y = Mathf.Max(0, index * 125);
        }

        private void DrawDiscoveryControls(float width)
        {
            GUI.Label(new Rect(12, 65, 56, 30), "Search", small);
            string query = GUI.TextField(new Rect(70, 65, width - 162, 30), discoverySearch, 80);
            if (query != discoverySearch) SetDiscoveryFilter(query, availableDiscoveriesOnly);
            if (Button(new Rect(width - 84, 65, 72, 30), "Clear")) SetDiscoveryFilter("", availableDiscoveriesOnly);
            if (Button(new Rect(12, 104, 172, 36), availableDiscoveriesOnly ? "Available only" : "All possibilities"))
                SetDiscoveryFilter(discoverySearch, !availableDiscoveriesOnly);
            if (Button(new Rect(192, 104, 164, 36), "Next discovery")) FocusNextDiscovery();
            GUI.Label(new Rect(364, 109, width - 376, 30), VisibleDiscoveryCount + " shown", small);
        }
    }
}