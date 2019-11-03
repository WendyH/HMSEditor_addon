using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DifferenceEngine {
    public enum DiffEngineLevel {
        FastImperfect,
        Medium,
        SlowPerfect
    }

    public class DiffEngine {
        public int MergeCost = 4;
        private IDiffList       _source    = null;
        private IDiffList       _dest      = null;
        private ArrayList       _matchList = null;
        private DiffStateList   _stateList = null;
        private DiffEngineLevel _level     = DiffEngineLevel.FastImperfect;

        public DiffEngine(string text1, string text2, bool byChars, bool trimEnd) {
            _source = new DiffList_TextFile(text1, byChars, trimEnd);
            _dest   = new DiffList_TextFile(text2, byChars, trimEnd);
        }

        public string GetSrcLineByIndex(int i) {
            if (i < 0 || i >= _source.Count()) return "";
            return ((TextLine)_source.GetByIndex(i)).Line;
        }

        public string GetDstLineByIndex(int i) {
            if (i < 0 || i >= _dest.Count()) return "";
            return ((TextLine)_dest.GetByIndex(i)).Line;
        }

        private int GetSourceMatchLength(int destIndex, int sourceIndex, int maxLength) {
            int matchCount;
            for (matchCount = 0; matchCount < maxLength; matchCount++) {
                if (_dest.GetByIndex(destIndex + matchCount).CompareTo(_source.GetByIndex(sourceIndex + matchCount)) != 0) {
                    break;
                }
            }
            return matchCount;
        }

        private void GetLongestSourceMatch(DiffState curItem, int destIndex, int destEnd, int sourceStart, int sourceEnd) {

            int maxDestLength = (destEnd - destIndex) + 1;
            int curLength     = 0;
            int curBestLength = 0;
            int curBestIndex  = -1;
            int maxLength     = 0;
            for (int sourceIndex = sourceStart; sourceIndex <= sourceEnd; sourceIndex++) {
                maxLength = Math.Min(maxDestLength, (sourceEnd - sourceIndex) + 1);
                if (maxLength <= curBestLength) {
                    //No chance to find a longer one any more
                    break;
                }
                curLength = GetSourceMatchLength(destIndex, sourceIndex, maxLength);
                if (curLength > curBestLength) {
                    //This is the best match so far
                    curBestIndex  = sourceIndex;
                    curBestLength = curLength;
                }
                //jump over the match
                sourceIndex += curBestLength;
            }
            //DiffState cur = _stateList.GetByIndex(destIndex);
            if (curBestIndex == -1) {
                curItem.SetNoMatch();
            } else {
                curItem.SetMatch(curBestIndex, curBestLength);
            }

        }

        private void ProcessRange(int destStart, int destEnd, int sourceStart, int sourceEnd) {
            int curBestIndex   = -1;
            int curBestLength  = -1;
            int maxPossibleDestLength = 0;
            DiffState curItem  = null;
            DiffState bestItem = null;
            for (int destIndex = destStart; destIndex <= destEnd; destIndex++) {
                maxPossibleDestLength = (destEnd - destIndex) + 1;
                if (maxPossibleDestLength <= curBestLength) {
                    //we won't find a longer one even if we looked
                    break;
                }
                curItem = _stateList.GetByIndex(destIndex);

                if (!curItem.HasValidLength(sourceStart, sourceEnd, maxPossibleDestLength)) {
                    //recalc new best length since it isn't valid or has never been done.
                    GetLongestSourceMatch(curItem, destIndex, destEnd, sourceStart, sourceEnd);
                }
                if (curItem.Status == DiffStatus.Matched) {
                    switch (_level) {
                        case DiffEngineLevel.FastImperfect:
                            if (curItem.Length > curBestLength) {
                                //this is longest match so far
                                curBestIndex  = destIndex;
                                curBestLength = curItem.Length;
                                bestItem      = curItem;
                            }
                            //Jump over the match 
                            destIndex += curItem.Length - 1;
                            break;
                        case DiffEngineLevel.Medium:
                            if (curItem.Length > curBestLength) {
                                //this is longest match so far
                                curBestIndex  = destIndex;
                                curBestLength = curItem.Length;
                                bestItem      = curItem;
                                //Jump over the match 
                                destIndex += curItem.Length - 1;
                            }
                            break;
                        default:
                            if (curItem.Length > curBestLength) {
                                //this is longest match so far
                                curBestIndex  = destIndex;
                                curBestLength = curItem.Length;
                                bestItem      = curItem;
                            }
                            break;
                    }
                }
            }
            if (curBestIndex < 0) {
                //we are done - there are no matches in this span
            } else {

                int sourceIndex = bestItem.StartIndex;
                _matchList.Add(DiffResultSpan.CreateNoChange(curBestIndex, sourceIndex, curBestLength));
                if (destStart < curBestIndex) {
                    //Still have more lower destination data
                    if (sourceStart < sourceIndex) {
                        //Still have more lower source data
                        // Recursive call to process lower indexes
                        ProcessRange(destStart, curBestIndex - 1, sourceStart, sourceIndex - 1);
                    }
                }
                int upperDestStart   = curBestIndex + curBestLength;
                int upperSourceStart = sourceIndex  + curBestLength;
                if (destEnd > upperDestStart) {
                    //we still have more upper dest data
                    if (sourceEnd > upperSourceStart) {
                        //set still have more upper source data
                        // Recursive call to process upper indexes
                        ProcessRange(upperDestStart, destEnd, upperSourceStart, sourceEnd);
                    }
                }
            }
        }

        public ArrayList ProcessDiff(DiffEngineLevel level) {
            ArrayList result = new ArrayList();

            if (_source == null || _dest == null)
                return result;

            _level = level;
            _matchList = new ArrayList();

            int dcount = _dest  .Count();
            int scount = _source.Count();

            if ((dcount > 0) && (scount > 0)) {
                _stateList = new DiffStateList(dcount);
                ProcessRange(0, dcount - 1, 0, scount - 1);
            }

            //Deal with the special case of empty files
            if (dcount == 0) {
                if (scount > 0) {
                    result.Add(DiffResultSpan.CreateDeleteSource(0, scount));
                }
                return result;
            } else {
                if (scount == 0) {
                    result.Add(DiffResultSpan.CreateAddDestination(0, dcount));
                    return result;
                }
            }

            _matchList.Sort();
            int curDest   = 0;
            int curSource = 0;
            DiffResultSpan last = null;

            //Process each match record
            foreach (DiffResultSpan drs in _matchList) {
                if ((!AddChanges(result, curDest, drs.DestIndex, curSource, drs.SourceIndex)) &&
                    (last != null)) {
                    last.AddLength(drs.Length);
                } else {
                    result.Add(drs);
                }
                curDest   = drs.DestIndex   + drs.Length;
                curSource = drs.SourceIndex + drs.Length;
                last = drs;
            }

            //Process any tail end data
            AddChanges(result, curDest, dcount, curSource, scount);

            return result;
        }


        private static bool AddChanges(ArrayList report, int curDest, int nextDest, int curSource, int nextSource) {
            bool success = false;
            int diffDest   = nextDest   - curDest;
            int diffSource = nextSource - curSource;
            int minDiff = 0;

            if (diffDest > 0) {
                if (diffSource > 0) {
                    minDiff = Math.Min(diffDest, diffSource);
                    report.Add(DiffResultSpan.CreateReplace(curDest, curSource, minDiff));
                    if (diffDest > diffSource) {
                        curDest += minDiff;
                        report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest - diffSource));
                    } else {
                        if (diffSource > diffDest) {
                            curSource += minDiff;
                            report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource - diffDest));
                        }
                    }
                } else {
                    report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest));
                }
                success = true;
            } else {
                if (diffSource > 0) {
                    report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource));
                    success = true;
                }
            }
            return success;
        }


        public void MergeShortSpan(ArrayList diffs) {
            bool   changes      = false;
            var    candidates   = new Stack<int>();
            string lastCandidat = string.Empty;
            int    lastIndex    = 0;
            int  pointer  = 0;
            bool pre_ins  = false;
            bool pre_del  = false;
            bool post_ins = false;
            bool post_del = false;
            while (pointer < diffs.Count) {
                DiffResultSpan diff = (DiffResultSpan)diffs[pointer];
                if (diff.Status == DiffResultSpanStatus.NoChange) {
                    if (diff.Length < MergeCost && (post_ins || post_del)) {
                        candidates.Push(pointer);
                        pre_ins = post_ins;
                        pre_del = post_del;
                        lastCandidat = GetSrcLineByIndex(diff.SourceIndex); // diff.text;
                        lastIndex    = diff.SourceIndex;
                    } else {
                        candidates.Clear();
                        lastCandidat = string.Empty;
                    }
                    post_ins = post_del = false;
                } else {
                    if (diff.Status == DiffResultSpanStatus.DeleteSource) {
                        post_del = true;
                        post_ins = true;
                    } else if (diff.Status == DiffResultSpanStatus.Replace) {
                        post_del = true;
                        post_ins = true;
                    } else {
                        post_ins = true;
                    }
                    if ((lastCandidat.Length != 0) && ((pre_ins && pre_del && post_ins && post_del) ||
                        ((lastCandidat.Length < this.MergeCost / 2) &&
                        ((pre_ins ? 1 : 0) + (pre_del ? 1 : 0) + (post_ins ? 1 : 0) + (post_del ? 1 : 0)) >= 2))) {

                        DiffResultSpan diffeq = (DiffResultSpan)diffs[candidates.Peek()];
                        diffs.Insert(candidates.Peek(), DiffResultSpan.CreateDeleteSource(lastIndex, lastCandidat.Length + diffeq.Length));
                        diffeq.Status = DiffResultSpanStatus.AddDestination;
                        candidates.Pop();
                        lastCandidat = string.Empty;
                        if (pre_ins && pre_del) {
                            post_ins = post_del = true;
                            candidates.Clear();
                        } else {
                            if (candidates.Count > 0)
                                candidates.Pop();
                            pointer = candidates.Count > 0 ? candidates.Peek() : -1;
                            post_ins = post_del = false;
                        }
                        changes = true;
                    }
                }
                pointer++;
            }
            if (changes) {
                CleanupMerge(diffs);
            }
        }

        public static void CleanupMerge(ArrayList diffs) {
            //int pointer = 0;
            //while (pointer < diffs.Count) {
            //    DiffResultSpan diff = (DiffResultSpan)diffs[pointer];
            //    pointer++;
            //}
        }

        public void CleanupSemantic(ArrayList diffs) {
            var    changes      = false;
            var    candidates   = new Stack<int>();  // Stack of indices where equalities are found.
            string lastCandidat = null;  // Always equal to equalities[equalitiesLength-1][1]
            int    lastIndex    = 0;
            int    lastLength   = 0;
            var    pointer      = 0; // Index of current position.
            var length_changes1 = 0; // Number of characters that changed prior to the equality.
            var length_changes2 = 0; // Number of characters that changed after the equality.
            while (pointer < diffs.Count) {
                DiffResultSpan diff = (DiffResultSpan)diffs[pointer];
                if (diff.Status == DiffResultSpanStatus.NoChange) {  // equality found
                    candidates.Push(pointer);
                    length_changes1 = length_changes2;
                    length_changes2 = 0;
                    lastCandidat    = GetSrcLineByIndex(diff.SourceIndex);
                    lastIndex       = diff.SourceIndex;
                    lastLength      = diff.Length;
                } else {  // an insertion or deletion
                    length_changes2 += diff.Length;
                    if (lastCandidat != null && (lastCandidat.Length <= length_changes1) && (lastCandidat.Length <= length_changes2)) {
                        DiffResultSpan diffeq = (DiffResultSpan)diffs[candidates.Peek()];
                        diffs.Insert(candidates.Peek(), DiffResultSpan.CreateDeleteSource(lastIndex, lastCandidat.Length + diffeq.Length));
                        diffeq.Status = DiffResultSpanStatus.AddDestination;
                        candidates.Pop();
                        if (candidates.Count > 0)
                            candidates.Pop();
                        pointer = candidates.Count > 0 ? candidates.Peek() : -1;
                        length_changes1 = 0;  // Reset the counters.
                        length_changes2 = 0;
                        lastCandidat    = null;
                        changes = true;
                    }
                }
                pointer++;
            }
            if (changes) {
                CleanupMerge(diffs);
            }
            CleanupSemanticLossless(diffs);
        }

        public void CleanupSemanticLossless(ArrayList diffs) {
            var pointer = 1;
            // Intentionally ignore the first and last element (don't need checking).
            while (pointer < diffs.Count - 1) {
                DiffResultSpan diff1 = (DiffResultSpan)diffs[pointer - 1];
                DiffResultSpan diff2 = (DiffResultSpan)diffs[pointer    ];
                DiffResultSpan diff3 = (DiffResultSpan)diffs[pointer + 1];

                if (diff1.Status == DiffResultSpanStatus.NoChange &&
                    diff3.Status == DiffResultSpanStatus.NoChange) {
                    // This is a single edit surrounded by equalities.
                    var equality1 = GetSrcLineByIndex(diff1.SourceIndex);
                    var edit      = GetSrcLineByIndex(diff2.SourceIndex);
                    var equality2 = GetSrcLineByIndex(diff3.SourceIndex);

                    // First, shift the edit as far left as possible.
                    var commonOffset = DiffEngine.diff_commonSuffix(equality1, edit);
                    if (commonOffset > 0) {
                        var commonString = edit.Substring(edit.Length - commonOffset);
                        equality1 = equality1.Substring(0, equality1.Length - commonOffset);
                        edit      = commonString + edit.Substring(0, edit.Length - commonOffset);
                        equality2 = commonString + equality2;
                    }

                    // Second, step character by character right, looking for the best fit.
                    var bestEquality1 = equality1;
                    var bestEdit      = edit;
                    var bestEquality2 = equality2;
                    var bestScore     = diff_cleanupSemanticScore(equality1, edit) +
                        diff_cleanupSemanticScore(edit, equality2);
                    while (edit.Length != 0 && equality2.Length != 0 && edit[0] == equality2[0]) {
                        equality1 += edit[0];
                        edit = edit.Substring(1) + equality2[0];
                        equality2 = equality2.Substring(1);
                        var score = diff_cleanupSemanticScore(equality1, edit) + diff_cleanupSemanticScore(edit, equality2);
                        // The >= encourages trailing rather than leading whitespace on edits.
                        if (score >= bestScore) {
                            bestScore     = score;
                            bestEquality1 = equality1;
                            bestEdit      = edit;
                            bestEquality2 = equality2;
                        }
                    }

                    if (GetSrcLineByIndex(diff1.SourceIndex) != bestEquality1) {
                        int indx = 0;
                        // We have an improvement, save it back to the diff.
                        if (bestEquality1.Length != 0) {
                            indx = (diff1.Length - bestEquality1.Length);
                            diff1.Length = bestEquality1.Length;
                        } else {
                            indx = -diff1.Length;
                            diffs.RemoveAt(pointer - 1);
                            pointer--;
                        }
                        diff2.SourceIndex += indx;
                        diff2.DestIndex   += indx;
                        diff2.Length = bestEdit.Length - indx;

                        if (bestEquality2.Length != 0) {
                            diff3.SourceIndex += indx;
                            diff3.DestIndex   += indx;
                            diff3.Length = bestEquality2.Length;
                        } else {
                            diffs.RemoveAt(pointer + 1);
                            pointer--;
                        }
                    }
                }
                pointer++;
            }
        }

        private int diff_cleanupSemanticScore(string one, string two) {
            if (one.Length == 0 || two.Length == 0) return 5;
            int score = 0;
            if (!Char.IsLetterOrDigit(one[one.Length - 1]) || !Char.IsLetterOrDigit(two[0])) {
                score++;
                if (Char.IsWhiteSpace(one[one.Length - 1]) || Char.IsWhiteSpace(two[0])) {
                    score++;
                    // Three points for line breaks.
                    if (Char.IsControl(one[one.Length - 1]) || Char.IsControl(two[0])) {
                        score++;
                        // Four points for blank lines.
                        if (BLANKLINEEND.IsMatch(one) || BLANKLINESTART.IsMatch(two)) {
                            score++;
                        }
                    }
                }
            }
            return score;
        }

        public static int diff_commonSuffix(string text1, string text2) {
            // Performance analysis: http://neil.fraser.name/news/2007/10/09/
            int text1_length = text1.Length;
            int text2_length = text2.Length;
            int n = Math.Min(text1.Length, text2.Length);
            for (int i = 1; i <= n; i++) {
                if (text1[text1_length - i] != text2[text2_length - i]) {
                    return i - 1;
                }
            }
            return n;
        }

        private Regex BLANKLINEEND   = new Regex("\\n\\r?\\n\\Z");
        private Regex BLANKLINESTART = new Regex("\\A\\r?\\n\\r?\\n");

    }
}
