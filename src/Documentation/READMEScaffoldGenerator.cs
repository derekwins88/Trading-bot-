using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SymbolicTrading.Documentation

{
    /*
     * Integration Notes:
     * - Instantiated by ThinkingEngine during boot; modules call RegisterModule()
     *   when they are discovered via InjectChaos() or AdvanceMarket().
     * - Generates markdown prior to ShardManager persistence and may be reset when
     *   StrategyAvatar rebuilds its environment.
     * - Results can be accessed by PulseRing to display documentation.
     * Typical usage:
     *   RegisterModule(typeof(SomeModule));
     *   GenerateAllDocumentation();
     * Lifecycle: instantiated once per run, mutated via RegisterModule, writes docs
     *   to disk, then can be serialized or discarded on reset.
     */
    public class READMEScaffoldGenerator
    {
        private readonly string _solutionRoot;
        private readonly List<Type> _moduleTypes = new();

        public READMEScaffoldGenerator(string solutionRoot)
        {
            _solutionRoot = solutionRoot;
        }

        public void RegisterModule(Type moduleType)
        {
            if (!_moduleTypes.Contains(moduleType))
            {
                _moduleTypes.Add(moduleType);
            }
        }

        public void GenerateAllDocumentation()
        {
            foreach (var type in _moduleTypes)
            {
                GenerateModuleDocumentation(type);
            }
            GenerateArchitectureOverview();
        }

        private void GenerateModuleDocumentation(Type moduleType)
        {
            var docs = new StringBuilder();
            var assembly = moduleType.Assembly;
            
            // Get module attributes
            var moduleAttr = moduleType.GetCustomAttribute<SymbolicModuleAttribute>();
            var namespaceSegments = moduleType.Namespace.Split('.');
            var moduleName = namespaceSegments.Length > 1 ? namespaceSegments[1] : "Core";
            
            // Header
            docs.AppendLine(f"# {moduleType.Name} Module");
            docs.AppendLine(f"**Namespace:** {moduleType.Namespace}  ");
            docs.AppendLine(f"**Category:** {moduleAttr?.Category ?? 'Uncategorized'}  ");
            docs.AppendLine(f"**Stability:** {moduleAttr?.Stability ?? 'Stable'}  ");
            docs.AppendLine();
            
            // Description from XML docs
            docs.AppendLine("## Overview");
            docs.AppendLine(GetClassSummary(moduleType) + "  ");
            docs.AppendLine();
            
            // Key Components
            docs.AppendLine("## Key Components");
            var publicTypes = assembly.GetTypes()
                .Where(t => t.Namespace == moduleType.Namespace && t.IsPublic)
                .OrderBy(t => t.Name);
            
            foreach (var type in publicTypes)
            {
                docs.AppendLine(f"### {type.Name}");
                docs.AppendLine(GetClassSummary(type) + "  ");
                
                // Methods
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => m.DeclaringType == type);
                
                if (methods.Any())
                {
                    docs.AppendLine("#### Methods");
                    foreach (var method in methods)
                    {
                        docs.AppendLine(f"- `{method.Name}`: {GetMethodSummary(method)}");
                    }
                }
                docs.AppendLine();
            }
            
            // Dependencies
            if (moduleAttr?.Dependencies?.Any() == true)
            {
                docs.AppendLine("## Dependencies");
                foreach (var dep in moduleAttr.Dependencies)
                {
                    docs.AppendLine(f"- {dep}");
                }
            }
            
            // Save to file
            var docsPath = Path.Combine(_solutionRoot, 'Docs', 'Modules', f"{moduleType.Name}.md");
            Directory.CreateDirectory(Path.GetDirectoryName(docsPath));
            File.WriteAllText(docsPath, docs.ToString());
        }

        private void GenerateArchitectureOverview()
        {
            var archDoc = new StringBuilder();
            archDoc.AppendLine("# Symbolic Trading System Architecture");
            archDoc.AppendLine("## Module Ecosystem");
            
            // Group modules by category
            var categories = _moduleTypes
                .Select(t => t.GetCustomAttribute<SymbolicModuleAttribute>()?.Category ?? 'Uncategorized')
                .Distinct()
                .OrderBy(c => c);
            
            foreach (var category in categories)
            {
                archDoc.AppendLine(f"### {category} Modules");
                var modules = _moduleTypes
                    .Where(t => (t.GetCustomAttribute<SymbolicModuleAttribute>()?.Category ?? 'Uncategorized') == category)
                    .OrderBy(t => t.Name);
                
                foreach (var module in modules)
                {
                    var attr = module.GetCustomAttribute<SymbolicModuleAttribute>();
                    archDoc.AppendLine(f"- **{module.Name}**: {attr?.Description ?? GetClassSummary(module)}");
                }
            }
            
            // Data Flow Diagram
            archDoc.AppendLine();
            archDoc.AppendLine("## Data Flow");
            archDoc.AppendLine("```mermaid");
            archDoc.AppendLine("graph TD");
            archDoc.AppendLine("    A[Market Data] --> B(Symbol Mapper)");
            archDoc.AppendLine("    B --> C{Glyph Stream}");
            archDoc.AppendLine("    C --> D[Thinking Engine]");
            archDoc.AppendLine("    D --> E[Axiom Processing]");
            archDoc.AppendLine("    E --> F[Trade Execution]");
            archDoc.AppendLine("    F --> G[Performance Tracking]");
            archDoc.AppendLine("    G --> H[Memory Capsules]");
            archDoc.AppendLine("    H --> D");
            archDoc.AppendLine("```");
            
            // Save architecture overview
            var archPath = Path.Combine(_solutionRoot, 'Docs', 'Architecture.md');
            File.WriteAllText(archPath, archDoc.ToString());
        }

        private string GetClassSummary(Type type)
        {
            // In a real implementation, this would parse XML documentation
            return type.GetCustomAttribute<SymbolicModuleAttribute>()?.Description                 ?? 'Symbolic trading system component';
        }

        private string GetMethodSummary(MethodInfo method)
        {
            // In a real implementation, this would parse XML documentation
            return method.GetCustomAttribute<SymbolicMethodAttribute>()?.Description                 ?? 'Performs core symbolic operations';
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SymbolicModuleAttribute : Attribute
    {
        public string Category { get; set; } = 'Core';
        public string Stability { get; set; } = 'Stable';
        public string Description { get; set; }
        public string[] Dependencies { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SymbolicMethodAttribute : Attribute
    {
        public string Description { get; set; }
    }
}
