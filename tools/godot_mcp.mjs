import { Client } from "../../godot-mcp/node_modules/@modelcontextprotocol/sdk/dist/client/index.js";
import { StdioClientTransport } from "../../godot-mcp/node_modules/@modelcontextprotocol/sdk/dist/client/stdio.js";

// Connect to the existing local stdio server; no cloud account is required.
const client = new Client({ name: "genesis-dev", version: "1.0.0" }, { capabilities: {} });
const transport = new StdioClientTransport({
  command: process.execPath,
  args: ["E:/Godot/godot-mcp/build/index.js"],
  env: { ...process.env, GODOT_PATH: "E:/Godot/Godot_v4.7.2-stable_win64_console.exe", DEBUG: "false" },
});
try {
  await client.connect(transport);
  const tool = process.argv[2];
  const result = !tool || tool === "list"
    ? await client.listTools()
    : await client.callTool({ name: tool, arguments: JSON.parse(process.argv[3] ?? "{}") });
  console.log(JSON.stringify(result, null, 2));
  if (result.isError) process.exitCode = 1;
} catch (error) {
  console.error(error);
  process.exitCode = 1;
} finally {
  await client.close();
}