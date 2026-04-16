#!/usr/bin/env bash
set -euo pipefail

export NL2GEO_LLM_PROVIDER="${NL2GEO_LLM_PROVIDER:-ollama}"
export NL2GEO_LLM_MODEL="${NL2GEO_LLM_MODEL:-llama3.1:8b}"
export NL2GEO_OLLAMA_BASE_URL="${NL2GEO_OLLAMA_BASE_URL:-http://localhost:11434}"

echo "Launching Rhino 8 with:"
echo "  NL2GEO_LLM_PROVIDER=$NL2GEO_LLM_PROVIDER"
echo "  NL2GEO_LLM_MODEL=$NL2GEO_LLM_MODEL"
echo "  NL2GEO_OLLAMA_BASE_URL=$NL2GEO_OLLAMA_BASE_URL"

open -a "Rhino 8"
