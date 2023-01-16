// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandLine.Text;
    using CommandLine;
    using AVOne.Tool.Resources;

    public class LocalizableSentenceBuilder : SentenceBuilder
    {
        public override Func<string> RequiredWord
        {
            get { return () => Resource.SentenceRequiredWord; }
        }

        public override Func<string> ErrorsHeadingText
        {
            // Cannot be pluralized
            get { return () => Resource.SentenceErrorsHeadingText; }
        }

        public override Func<string> UsageHeadingText
        {
            get { return () => Resource.SentenceUsageHeadingText; }
        }

        public override Func<bool, string> HelpCommandText
        {
            get
            {
                return isOption => isOption
                    ? Resource.SentenceHelpCommandTextOption
                    : Resource.SentenceHelpCommandTextVerb;
            }
        }

        public override Func<bool, string> VersionCommandText
        {
            get { return _ => Resource.SentenceVersionCommandText; }
        }

        public override Func<Error, string> FormatError
        {
            get
            {
                return error =>
                {
                    switch (error.Tag)
                    {
                        case ErrorType.BadFormatTokenError:
                            return string.Format(Resource.SentenceBadFormatTokenError, ((BadFormatTokenError)error).Token);
                        case ErrorType.MissingValueOptionError:
                            return string.Format(Resource.SentenceMissingValueOptionError, ((MissingValueOptionError)error).NameInfo.NameText);
                        case ErrorType.UnknownOptionError:
                            return string.Format(Resource.SentenceUnknownOptionError, ((UnknownOptionError)error).Token);
                        case ErrorType.MissingRequiredOptionError:
                            var errMisssing = ((MissingRequiredOptionError)error);
                            return errMisssing.NameInfo.Equals(NameInfo.EmptyName)
                                       ? Resource.SentenceMissingRequiredOptionError
                                       : string.Format(Resource.SentenceMissingRequiredOptionError, errMisssing.NameInfo.NameText);
                        case ErrorType.BadFormatConversionError:
                            var badFormat = ((BadFormatConversionError)error);
                            return badFormat.NameInfo.Equals(NameInfo.EmptyName)
                                       ? Resource.SentenceBadFormatConversionErrorValue
                                       : string.Format(Resource.SentenceBadFormatConversionErrorOption, badFormat.NameInfo.NameText);
                        case ErrorType.SequenceOutOfRangeError:
                            var seqOutRange = ((SequenceOutOfRangeError)error);
                            return seqOutRange.NameInfo.Equals(NameInfo.EmptyName)
                                       ? Resource.SentenceSequenceOutOfRangeErrorValue
                                       : string.Format(Resource.SentenceSequenceOutOfRangeErrorOption,
                                            seqOutRange.NameInfo.NameText);
                        case ErrorType.BadVerbSelectedError:
                            return string.Format(Resource.SentenceBadVerbSelectedError, ((BadVerbSelectedError)error).Token);
                        case ErrorType.NoVerbSelectedError:
                            return Resource.SentenceNoVerbSelectedError;
                        case ErrorType.RepeatedOptionError:
                            return string.Format(Resource.SentenceRepeatedOptionError, ((RepeatedOptionError)error).NameInfo.NameText);
                        case ErrorType.SetValueExceptionError:
                            var setValueError = (SetValueExceptionError)error;
                            return string.Format(Resource.SentenceSetValueExceptionError, setValueError.NameInfo.NameText, setValueError.Exception.Message);
                    }
                    throw new InvalidOperationException();
                };
            }
        }

        public override Func<IEnumerable<MutuallyExclusiveSetError>, string> FormatMutuallyExclusiveSetErrors
        {
            get
            {
                return errors =>
                {
                    var bySet = from e in errors
                                group e by e.SetName into g
                                select new { SetName = g.Key, Errors = g.ToList() };

                    var msgs = bySet.Select(
                        set =>
                        {
                            var names = string.Join(
                                string.Empty,
                                (from e in set.Errors select string.Format("'{0}', ", e.NameInfo.NameText)).ToArray());
                            var namesCount = set.Errors.Count();

                            var incompat = string.Join(
                                string.Empty,
                                (from x in
                                     (from s in bySet where !s.SetName.Equals(set.SetName) from e in s.Errors select e)
                                    .Distinct()
                                 select string.Format("'{0}', ", x.NameInfo.NameText)).ToArray());
                            //TODO: Pluralize by namesCount
                            return
                                string.Format(Resource.SentenceMutuallyExclusiveSetErrors,
                                    names.Substring(0, names.Length - 2), incompat.Substring(0, incompat.Length - 2));
                        }).ToArray();
                    return string.Join(Environment.NewLine, msgs);
                };
            }
        }

        public override Func<string> OptionGroupWord => () => string.Empty;
    }
}
