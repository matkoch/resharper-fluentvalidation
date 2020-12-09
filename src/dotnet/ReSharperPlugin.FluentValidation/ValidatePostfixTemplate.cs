using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.DocumentModel.DataContext;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.CSharp.PostfixTemplates;
using JetBrains.ReSharper.Feature.Services.CSharp.PostfixTemplates.Behaviors;
using JetBrains.ReSharper.Feature.Services.CSharp.PostfixTemplates.Contexts;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Feature.Services.PostfixTemplates;
using JetBrains.ReSharper.Feature.Services.PostfixTemplates.Contexts;
using JetBrains.ReSharper.Feature.Services.PostfixTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.Refactorings;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Refactorings.CSharp.IntroduceVariable;
using JetBrains.ReSharper.Refactorings.IntroduceVariable;
using JetBrains.ReSharper.Refactorings.IntroduceVariable.Impl;
using JetBrains.TextControl;
using JetBrains.TextControl.DataContext;
using JetBrains.Util;

namespace ReSharperPlugin.FluentValidation
{
    [PostfixTemplate("validate", "Validates an object", "expr.Validate()")]
    public class ValidatePostfixTemplate : CSharpPostfixTemplate
    {
        public override PostfixTemplateInfo TryCreateInfo(CSharpPostfixTemplateContext context)
        {
            var withValuesContexts = CSharpPostfixUtils.FindExpressionWithValuesContexts(context);
            var validatorTypes = GetPossibleValidatorTypes(context, withValuesContexts);

            return validatorTypes.Length == 0
                ? null
                : new ValidatePostfixTemplateInfo(withValuesContexts, validatorTypes, availableInPreciseMode: true);
        }

        private static ITypeElement[] GetPossibleValidatorTypes(
            CSharpPostfixTemplateContext context,
            CSharpPostfixExpressionContext[] withValuesContexts)
        {
            var name = (withValuesContexts.FirstOrDefault()?.ExpressionType as IDeclaredType)?.GetClrName().ShortName;
            var psiServices = context.PsiModule.GetPsiServices();
            var symbolScope =
                psiServices.Symbols.GetSymbolScope(context.PsiModule, withReferences: true, caseSensitive: false);
            return symbolScope.GetElementsByShortName(name + "Validator").OfType<ITypeElement>().ToArray();
        }

        public override PostfixTemplateBehavior CreateBehavior(
            PostfixTemplateInfo info)
        {
            return new ValidatePostfixBehavior(info);
        }

        private class ValidatePostfixTemplateInfo : PostfixTemplateInfo
        {
            public ITypeElement[] ValidatorTypes { get; }

            public ValidatePostfixTemplateInfo(
                [NotNull] IEnumerable<PostfixExpressionContext> expressions,
                ITypeElement[] validatorTypes,
                PostfixTemplateTarget target = PostfixTemplateTarget.Expression,
                bool availableInPreciseMode = true,
                string matchingText = null)
                : base("validate", expressions, target, availableInPreciseMode, matchingText)
            {
                ValidatorTypes = validatorTypes;
            }
        }

        private sealed class ValidatePostfixBehavior : CSharpStatementPostfixTemplateBehavior<ICSharpStatement>
        {
            public ValidatePostfixBehavior([NotNull] PostfixTemplateInfo info)
                : base(info)
            {
            }

            protected override string ExpressionSelectTitle => "Select object to validate";

            // protected override ICSharpExpression CreateExpression(
            //     CSharpElementFactory factory,
            //     ICSharpStatement expression)
            // {
            //     return factory.CreateExpression("$0.Validate(null)", (object) expression);
            // }

            // protected override void AfterComplete(ITextControl textControl, ICSharpStatement expression, Suffix suffix)
            // {
            //     // if (expressionStatement != null && expressionStatement.Semicolon == null)
            //     // {
            //     //     var documentRange = expression.GetDocumentRange();
            //     //     if (documentRange.IsValid())
            //     //     {
            //     //         var endOffset = documentRange.TextRange.EndOffset;
            //     //         textControl.Document.InsertText(endOffset, ";");
            //     //         textControl.Caret.MoveTo(endOffset + 1, CaretVisualPlacement.DontScrollIfVisible);
            //     //         return;
            //     //     }
            //     // }
            //     //
            //     // base.AfterComplete(textControl, expression, suffix);
            //     // suffix.Playback(textControl);
            // }

            protected override ICSharpStatement CreateStatement(CSharpElementFactory factory,
                ICSharpExpression expression)
            {
                var info = (ValidatePostfixTemplateInfo) Info;
                var type = TypeFactory.CreateType(info.ValidatorTypes.First());
                return factory.CreateStatement("new $0().Validate($1);", type, expression);
            }

            // protected override ICSharpStatement DecorateStatement(CSharpElementFactory factory,
            //     ICSharpStatement statement)
            // {
            //     var info = (ValidatePostfixTemplateInfo) Info;
            //     var type = TypeFactory.CreateType(info.ValidatorTypes.First());
            //     var newStatement = factory.CreateStatement("var validator = new $0();", type);
            //     var statementsOwner = StatementsOwnerNavigator.GetByStatement(statement);
            //     return statementsOwner.AddStatementBefore(newStatement, statement);
            // }

            protected override void AfterComplete(ITextControl textControl, ICSharpStatement statement, Suffix suffix)
            {
                var expressionMarker = statement.GetDocumentRange().CreateRangeMarker();
                var solution = statement.GetSolution();
                ExecuteRefactoring(textControl, statement.Descendants<IObjectCreationExpression>().First(), () =>
                {
                    var documentRange = expressionMarker.DocumentRange;
                    if (!documentRange.IsValid())
                        return;
                    solution.GetPsiServices().Files.CommitAllDocuments();

                    var element = TextControlToPsi.GetElement<ICSharpStatement>(solution, documentRange.EndOffset);
                    if (element == null)
                        return;

                    textControl.Caret.MoveTo(element.GetDocumentRange().TextRange.EndOffset,
                        CaretVisualPlacement.DontScrollIfVisible);
                });
                // base.AfterComplete(textControl, statement, suffix);
            }

            private static void ExecuteRefactoring(
                [NotNull] ITextControl textControl,
                [NotNull] ICSharpExpression expression,
                [CanBeNull] Action executeAfter = null)
            {
                const string actionId = IntroVariableAction.ACTION_ID;

                var solution = expression.GetSolution();
                var document = textControl.Document;

                var expressionRange = expression.GetDocumentRange()
                    .TextRange;
                textControl.Selection.SetRange(expressionRange);

                var rules = DataRules
                    .AddRule(actionId, ProjectModelDataConstants.SOLUTION, solution)
                    .AddRule(actionId, DocumentModelDataConstants.DOCUMENT, document)
                    .AddRule(actionId, TextControlDataConstants.TEXT_CONTROL, textControl);

                var settingsStore = expression.GetSettingsStoreWithEditorConfig();
                var multipleOccurrences = settingsStore.GetValue(PostfixTemplatesSettingsAccessor.SearchVarOccurrences);

                // note: uber ugly code down here
                using (var definition = Lifetime.Define(Lifetime.Eternal, actionId))
                {
                    var dataContexts = solution.GetComponent<DataContexts>();
                    var dataContext = dataContexts.CreateWithDataRules(definition.Lifetime, rules);

                    var workflow = new IntroduceVariableWorkflow(solution, actionId);
                    RefactoringActionUtil.ExecuteRefactoring(dataContext, workflow);

                    var finishedAction = executeAfter;
                    if (finishedAction != null)
                    {
                        var currentSession = HotspotSessionExecutor.Instance.CurrentSession;
                        if (currentSession != null) // ugly hack
                        {
                            currentSession.HotspotSession.Closed.Advise(Lifetime.Eternal,
                                (e) =>
                                {
                                    if (e.TerminationType == TerminationType.Finished)
                                    {
                                        finishedAction();
                                    }
                                });
                        }
                        else
                        {
                            finishedAction();
                        }
                    }
                }
            }


            //     protected override void AfterComplete(
            //         ITextControl textControl,
            //         IObjectCreationExpression expression,
            //         Suffix suffix)
            //     {
            //         if ((TypeUtils.CanInstantiateType(CSharpTypeFactory.CreateType(expression.TypeUsage), (ITreeNode) expression) & CanInstantiate.ConstructorWithParameters) != CanInstantiate.No)
            //         {
            //             IRangeMarker argumentsMarker = expression.LPar.GetDocumentRange().SetEndTo(expression.RPar.GetDocumentRange().TextRange.EndOffset).CreateRangeMarker();
            //             bool invokeParameterInfo = expression.GetSettingsStoreWithEditorConfig().GetValue<PostfixTemplatesSettings, bool>(PostfixTemplatesSettingsAccessor.InvokeParameterInfo);
            //             ISolution solution = expression.GetSolution();
            //             IntroduceVariableTemplate.ExecuteRefactoring(textControl, (ICSharpExpression) expression, (Action) (() =>
            //             {
            //                 TextRange range = argumentsMarker.Range;
            //                 if (!range.IsValid)
            //                     return;
            //                 solution.GetPsiServices().Files.CommitAllDocuments();
            //                 textControl.Caret.MoveTo(range.StartOffset + range.Length / 2, CaretVisualPlacement.DontScrollIfVisible);
            //                 if (!invokeParameterInfo)
            //                     return;
            //                 LookupUtil.ShowParameterInfo(solution, textControl, this.Info.ExecutionContext.CreateLookupItemsOwner());
            //             }));
            //         }
            //         else
            //             IntroduceVariableTemplate.ExecuteRefactoring(textControl, (ICSharpExpression) expression);
            //     }
            // }
        }
    }
}
