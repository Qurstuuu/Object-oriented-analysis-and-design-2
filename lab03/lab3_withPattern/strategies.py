import os
import hashlib
import time
from datetime import datetime
from abc import ABC, abstractmethod

class FileSearcher(ABC):
    def search(self, root_path):
        all_files = self._walk_directory(root_path)
        matched_files = [f for f in all_files if self.matches_criteria(f)]
        results = []
        for filepath in matched_files:
            info = self.extract_info(filepath)
            if info:
                results.append(info)
        return self.sort_results(results)
    
    def _walk_directory(self, root_path):
        files = []
        for dirpath, _, filenames in os.walk(root_path):
            for filename in filenames:
                files.append(os.path.join(dirpath, filename))
        return files
    
    @abstractmethod
    def matches_criteria(self, filepath):
        pass
    
    @abstractmethod
    def extract_info(self, filepath):
        pass
    
    def sort_results(self, results):
        return sorted(results, key=lambda x: x.get('path', ''))


class DuplicateFinder(FileSearcher):
    
    def matches_criteria(self, filepath):
        return True
    
    def extract_info(self, filepath):
        try:
            with open(filepath, 'rb') as f:
                file_hash = hashlib.md5(f.read()).hexdigest()
            return {
                'path': filepath,
                'hash': file_hash,
                'size': os.path.getsize(filepath)
            }
        except (PermissionError, OSError):
            return None
    
    def sort_results(self, results):
        from collections import defaultdict
        groups = defaultdict(list)
        for item in results:
            if item:
                groups[item['hash']].append(item)
        
        duplicates = []
        for group in groups.values():
            if len(group) > 1:
                duplicates.append(group)
        return duplicates


class OldFileSearcher(FileSearcher):
    
    def __init__(self, days_old=30):
        self.cutoff = time.time() - (days_old * 86400)
    
    def matches_criteria(self, filepath):
        try:
            return os.path.getmtime(filepath) < self.cutoff
        except (PermissionError, OSError):
            return False
    
    def extract_info(self, filepath):
        try:
            mtime = os.path.getmtime(filepath)
            days = (time.time() - mtime) // 86400
            return {
                'path': filepath,
                'info': f'{int(days)} days ago',
                'days_old': int(days)
            }
        except (PermissionError, OSError):
            return None


class TextInFileSearcher(FileSearcher):
    
    def __init__(self, search_text, case_sensitive=False):
        self.search_text = search_text if case_sensitive else search_text.lower()
        self.case_sensitive = case_sensitive
    
    def matches_criteria(self, filepath):
        text_extensions = {'.txt', '.py', '.md', '.csv', '.json', '.xml', '.html', '.cpp', '.h', '.java'}
        return os.path.splitext(filepath)[1].lower() in text_extensions
    
    def extract_info(self, filepath):
        try:
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
                if not self.case_sensitive:
                    content = content.lower()
                
                if self.search_text in content:
                    lines = content.split('\n')
                    for i, line in enumerate(lines):
                        if self.search_text in line:
                            line_preview = line[:80] + '...' if len(line) > 80 else line
                            return {
                                'path': filepath,
                                'info': f'Line {i+1}: {line_preview}',
                                'line_number': i + 1,
                                'line': line_preview
                            }
        except (PermissionError, UnicodeDecodeError):
            pass
        return None


class SizeBasedSearcher(FileSearcher):
    
    def __init__(self, min_mb=10):
        self.min_bytes = min_mb * 1024 * 1024
    
    def matches_criteria(self, filepath):
        try:
            return os.path.getsize(filepath) >= self.min_bytes
        except (PermissionError, OSError):
            return False
    
    def extract_info(self, filepath):
        try:
            size = os.path.getsize(filepath)
            return {
                'path': filepath,
                'info': f'{size / (1024 * 1024):.2f} MB',
                'size_mb': round(size / (1024 * 1024), 2)
            }
        except (PermissionError, OSError):
            return None