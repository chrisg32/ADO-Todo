using System;

namespace ADOTodo.Models
{
    public class PrSettings
    {
        public PrSettings() : this(ThreadFilterLevel.All){ }
        public PrSettings(ThreadFilterLevel level)
        {
            SetLevel(level);
        }

        private void SetLevel(ThreadFilterLevel level)
        {
            _level = level;
            switch (level)
            {
                case ThreadFilterLevel.None:
                    _none = true;
                    _allOpenCommentsOnThreadsMentioningMe = false;
                    _allOpenCommentsOnThreadsICommentedOn = false;
                    _allOpenCommentsOnThreadsIStarted = false;
                    _allOpenComments = false;
                    break;
                case ThreadFilterLevel.Mentions:
                    _none = false;
                    _allOpenCommentsOnThreadsMentioningMe = true;
                    _allOpenCommentsOnThreadsICommentedOn = false;
                    _allOpenCommentsOnThreadsIStarted = false;
                    _allOpenComments = false;
                    break;
                case ThreadFilterLevel.Comments:
                    _none = false;
                    _allOpenCommentsOnThreadsMentioningMe = false;
                    _allOpenCommentsOnThreadsICommentedOn = true;
                    _allOpenCommentsOnThreadsIStarted = false;
                    _allOpenComments = false;
                    break;
                case ThreadFilterLevel.Threads:
                    _none = false;
                    _allOpenCommentsOnThreadsMentioningMe = false;
                    _allOpenCommentsOnThreadsICommentedOn = false;
                    _allOpenCommentsOnThreadsIStarted = true;
                    _allOpenComments = false;
                    break;
                case ThreadFilterLevel.All:
                    _none = false;
                    _allOpenCommentsOnThreadsMentioningMe = false;
                    _allOpenCommentsOnThreadsICommentedOn = false;
                    _allOpenCommentsOnThreadsIStarted = false;
                    _allOpenComments = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private ThreadFilterLevel _level;

        public ThreadFilterLevel Level
        {
            get => _level;
            set => SetLevel(value);
        }

        private bool _allOpenComments;
        public bool AllOpenComments
        {
            get => _allOpenComments;
            set
            {
                _allOpenComments = value;
                if(value) SetLevel(ThreadFilterLevel.All);
            }
        }

        private bool _allOpenCommentsOnThreadsIStarted;
        public bool AllOpenCommentsOnThreadsIStarted
        {
            get => _allOpenCommentsOnThreadsIStarted;
            set
            {
                _allOpenCommentsOnThreadsIStarted = value;
                if(value) SetLevel(ThreadFilterLevel.Threads);
            }
        }

        private bool _allOpenCommentsOnThreadsICommentedOn;
        public bool AllOpenCommentsOnThreadsICommentedOn
        {
            get => _allOpenCommentsOnThreadsICommentedOn;
            set
            {
                _allOpenCommentsOnThreadsICommentedOn = value;
                if(value) SetLevel(ThreadFilterLevel.Comments);
            }
        }

        private bool _allOpenCommentsOnThreadsMentioningMe;
        public bool AllOpenCommentsOnThreadsMentioningMe
        {
            get => _allOpenCommentsOnThreadsMentioningMe;
            set
            {
                _allOpenCommentsOnThreadsMentioningMe = value;
                if(value) SetLevel(ThreadFilterLevel.Mentions);
            }
        }

        private bool _none;

        public bool None
        {
            get => _none;
            set
            {
                _none = value;
                if(value) SetLevel(ThreadFilterLevel.None);
            }
        }
    }
}