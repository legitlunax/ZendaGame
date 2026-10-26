class_name AIService
extends Node

enum Provider {
	OPENAI_CHATGPT,
	MINIMAX
}

signal request_started(provider: Provider, prompt: String)
signal response_received(provider: Provider, response_text: String, raw_response: Dictionary)
signal request_failed(provider: Provider, error_message: String)

@export var default_provider: Provider = Provider.OPENAI_CHATGPT
@export var chatgpt_model: String = "gpt-4o-mini"
@export var minimax_model: String = "abab6.5s-chat"
@export var default_temperature: float = 0.7
@export var default_max_tokens: int = 1024

var _http_request: HTTPRequest
var _current_provider: Provider = Provider.OPENAI_CHATGPT

func _ready() -> void:
	_http_request = HTTPRequest.new()
	_http_request.timeout = 30.0
	add_child(_http_request)
	_http_request.request_completed.connect(_on_request_completed)

func ask_chatgpt(prompt: String, system_prompt: String = "", custom_key: String = "") -> void:
	var api_key: String = custom_key if not custom_key.is_empty() else AIConfig.get_openai_api_key()
	if api_key.is_empty():
		request_failed.emit(Provider.OPENAI_CHATGPT, "Missing OpenAI API Key. Set OPENAI_API_KEY environment variable or save credentials in AIConfig.")
		return
	
	_current_provider = Provider.OPENAI_CHATGPT
	request_started.emit(_current_provider, prompt)
	
	var url := "https://api.openai.com/v1/chat/completions"
	var headers: PackedStringArray = [
		"Content-Type: application/json",
		"Authorization: Bearer " + api_key
	]
	
	var messages: Array[Dictionary] = []
	if not system_prompt.is_empty():
		messages.append({"role": "system", "content": system_prompt})
	messages.append({"role": "user", "content": prompt})
	
	var payload := {
		"model": chatgpt_model,
		"messages": messages,
		"temperature": default_temperature,
		"max_tokens": default_max_tokens
	}
	
	var body := JSON.stringify(payload)
	var err := _http_request.request(url, headers, HTTPClient.METHOD_POST, body)
	if err != OK:
		request_failed.emit(Provider.OPENAI_CHATGPT, "Failed to dispatch HTTP request. Code: %d" % err)

func ask_minimax(prompt: String, system_prompt: String = "", custom_key: String = "") -> void:
	var api_key: String = custom_key if not custom_key.is_empty() else AIConfig.get_minimax_api_key()
	if api_key.is_empty():
		request_failed.emit(Provider.MINIMAX, "Missing MiniMax API Key. Set MINIMAX_API_KEY environment variable or save credentials in AIConfig.")
		return
	
	_current_provider = Provider.MINIMAX
	request_started.emit(_current_provider, prompt)
	
	var url := "https://api.minimax.chat/v1/text/chatcompletion_v2"
	var headers: PackedStringArray = [
		"Content-Type: application/json",
		"Authorization: Bearer " + api_key
	]
	
	var messages: Array[Dictionary] = []
	if not system_prompt.is_empty():
		messages.append({"role": "system", "content": system_prompt})
	messages.append({"role": "user", "content": prompt})
	
	var payload := {
		"model": minimax_model,
		"messages": messages,
		"temperature": default_temperature,
		"max_tokens": default_max_tokens
	}
	
	var body := JSON.stringify(payload)
	var err := _http_request.request(url, headers, HTTPClient.METHOD_POST, body)
	if err != OK:
		request_failed.emit(Provider.MINIMAX, "Failed to dispatch MiniMax HTTP request. Code: %d" % err)

func ask(prompt: String, system_prompt: String = "", provider: Provider = default_provider) -> void:
	match provider:
		Provider.OPENAI_CHATGPT:
			ask_chatgpt(prompt, system_prompt)
		Provider.MINIMAX:
			ask_minimax(prompt, system_prompt)

func _on_request_completed(result: int, response_code: int, _headers: PackedStringArray, body: PackedByteArray) -> void:
	if result != HTTPRequest.RESULT_SUCCESS:
		request_failed.emit(_current_provider, "HTTP request failed with transport error: %d (HTTP %d)" % [result, response_code])
		return
	
	var response_text := body.get_string_from_utf8()
	var parsed: Variant = JSON.parse_string(response_text)
	
	if not (parsed is Dictionary):
		request_failed.emit(_current_provider, "Invalid JSON received: %s" % response_text)
		return
	
	var dict: Dictionary = parsed
	if response_code < 200 or response_code >= 300:
		var err_msg := "HTTP error %d" % response_code
		if dict.has("error"):
			err_msg = str(dict["error"])
		request_failed.emit(_current_provider, err_msg)
		return
	
	var text_content := ""
	if dict.has("choices") and dict["choices"] is Array and dict["choices"].size() > 0:
		var choice: Dictionary = dict["choices"][0]
		if choice.has("message") and choice["message"] is Dictionary:
			text_content = choice["message"].get("content", "")
		elif choice.has("text"):
			text_content = choice.get("text", "")
	elif dict.has("reply"):
		text_content = str(dict["reply"])
	
	response_received.emit(_current_provider, text_content, dict)
