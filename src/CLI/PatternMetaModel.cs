﻿using System.Collections.Generic;
using automate.Extensions;

namespace automate
{
    internal class PatternMetaModel : INamedEntity, IElementContainer, IAutomationContainer, ICustomizableEntity
    {
        public PatternMetaModel(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsIdentifier, nameof(name),
                ExceptionMessages.Validations_InvalidIdentifier);
            
            Name = name;
            Id = IdGenerator.Create();
            Attributes = new List<Attribute>();
            Elements = new List<Element>();
            CodeTemplates = new List<CodeTemplate>();
        }

        /// <summary>
        ///     For serialization
        /// </summary>
        public PatternMetaModel()
        {
        }

        public List<CodeTemplate> CodeTemplates { get; set; }

        public List<Attribute> Attributes { get; set; }

        public List<Element> Elements { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }
    }
}