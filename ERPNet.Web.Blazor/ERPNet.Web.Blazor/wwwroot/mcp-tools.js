window.erpnetMcp = {

    /**
     * Registra los tools globales de ERPNet (disponibles en todas las páginas).
     * Se llama una vez tras cargar la app desde McpToolService.
     */
    registerGlobalTools: function () {
        if (!navigator.modelContext) return;

        navigator.modelContext.provideContext({
            tools: [
                {
                    name: "erpnet_info",
                    description: "Devuelve información general del sistema ERPNet: nombre, versión y módulos disponibles.",
                    inputSchema: { type: "object", properties: {} },
                    annotations: { readOnlyHint: true },
                    async execute() {
                        return {
                            content: [{
                                type: "text",
                                text: JSON.stringify({
                                    sistema: "ERPNet",
                                    version: "1.0.0",
                                    modulos: ["RRHH", "Maquinaria", "Usuarios", "Roles"]
                                })
                            }]
                        };
                    }
                },
                {
                    name: "get_usuario_actual",
                    description: "Devuelve el nombre y email del usuario que tiene la sesión activa.",
                    inputSchema: { type: "object", properties: {} },
                    annotations: { readOnlyHint: true },
                    async execute() {
                        try {
                            const res = await fetch("/api/auth/account", { credentials: "include" });
                            if (!res.ok) return { content: [{ type: "text", text: "No autenticado" }] };
                            const data = await res.json();
                            return { content: [{ type: "text", text: JSON.stringify(data) }] };
                        } catch (e) {
                            return { content: [{ type: "text", text: `Error: ${e.message}` }], isError: true };
                        }
                    }
                }
            ]
        });
    },

    /**
     * Registra un tool individual. Llamado desde McpToolService via JS interop.
     * @param {object} tool - { name, description, inputSchema, readOnly, dotnetRef, methodName }
     */
    registerTool: function (tool) {
        if (!navigator.modelContext) return;

        navigator.modelContext.registerTool({
            name: tool.name,
            description: tool.description,
            inputSchema: tool.inputSchema,
            annotations: { readOnlyHint: tool.readOnly ?? false },
            async execute(input, client) {
                // Operaciones de escritura requieren confirmación humana
                if (!tool.readOnly) {
                    let confirmed = false;
                    if (client?.requestUserInteraction) {
                        // API nativa (Chrome 146+ con flag)
                        confirmed = await client.requestUserInteraction(async () => {
                            return await tool.dotnetRef.invokeMethodAsync("RequestConfirmation", tool.name, input);
                        });
                    } else {
                        // Fallback: diálogo nativo del browser
                        confirmed = window.confirm(`¿Confirmar operación "${tool.name}"?`);
                    }
                    if (!confirmed) {
                        return { content: [{ type: "text", text: "Operación cancelada por el usuario." }] };
                    }
                }

                const result = await tool.dotnetRef.invokeMethodAsync("ExecuteTool", tool.name, input);
                return { content: [{ type: "text", text: result }] };
            }
        });
    },

    /**
     * Desregistra un tool por nombre.
     */
    unregisterTool: function (name) {
        if (!navigator.modelContext) return;
        try {
            navigator.modelContext.unregisterTool(name);
        } catch (_) { }
    },

    /**
     * Devuelve true si navigator.modelContext está disponible.
     */
    isAvailable: function () {
        return !!navigator.modelContext;
    }
};
