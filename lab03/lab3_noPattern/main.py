import os
import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import hashlib
import time
import subprocess
import platform
from datetime import datetime

class SearchApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Поиск файлов")
        self.root.geometry("600x500")
        self.setup_ui()
    
    def setup_ui(self):
        folder_frame = tk.Frame(self.root)
        folder_frame.pack(anchor='w', padx=10, pady=5)
        tk.Label(folder_frame, text="Папка для поиска:").pack(padx=(0,10), anchor="w")
        self.path_var = tk.StringVar()
        tk.Entry(folder_frame, textvariable=self.path_var, width=40).pack(side=tk.LEFT, padx=(0,10))
        tk.Button(folder_frame, text="Обзор...", command=self.select_folder).pack(side=tk.LEFT)
        
        self.strategy_var = tk.StringVar(value="duplicate")
        folder_strategy = tk.Frame(self.root)
        folder_strategy.pack(anchor="w", padx=10, pady=5)
        tk.Label(folder_strategy, text="Тип поиска:").pack(pady=5, anchor="w")
        self.strategy_var = tk.StringVar(value="duplicate")
        for text, val in [("Дубликаты", "duplicate"), ("Старые (>30 дней)", "old"),
                          ("Поиск текста", "text"), (">10 МБ", "size")]:
            tk.Radiobutton(folder_strategy, text=text, variable=self.strategy_var, 
                          value=val, command=self.on_strategy_change).pack(side=tk.LEFT)
        
        self.params_frame = tk.Frame(self.root)
        self.params_frame.pack(anchor='w', padx=10, pady=5)
        self.on_strategy_change()
        
        self.search_btn = tk.Button(self.root, text="Найти", command=self.search, bg="orange")
        self.search_btn.pack()
        
        frame = tk.Frame(self.root)
        frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        scrollbar = tk.Scrollbar(frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.tree = ttk.Treeview(frame, yscrollcommand=scrollbar.set, show="tree headings")
        self.tree.heading("#0", text="Файл")
        self.tree["columns"] = ("info",)
        self.tree.heading("info", text="Информация")
        self.tree.column("#0", width=500)
        self.tree.column("info", width=250)
        self.tree.pack(fill=tk.BOTH, expand=True)
        scrollbar.config(command=self.tree.yview)
        
        self.tree.bind("<Double-1>", self.open_file)

    def open_file(self, event):
        selected = self.tree.selection()
        if not selected:
            return
        
        filepath = self.tree.item(selected[0], "text")
        
        if not filepath or not os.path.exists(filepath):
            messagebox.showerror("Ошибка", f"Файл не найден:\n{filepath}")
            return
        
        try:
            if platform.system() == "Windows":
                os.startfile(filepath)
            elif platform.system() == "Darwin":  # macOS
                subprocess.run(["open", filepath])
            else:  # Unix
                subprocess.run(["xdg-open", filepath])
        except Exception as e:
            messagebox.showerror("Ошибка", f"Не удалось открыть файл:\n{str(e)}")

    def select_folder(self):
        folder = filedialog.askdirectory()
        if folder:
            self.path_var.set(folder)
    
    def on_strategy_change(self):
        for w in self.params_frame.winfo_children():
            w.destroy()
        if self.strategy_var.get() == "text":
            tk.Label(self.params_frame, text="Текст:").pack(side=tk.LEFT)
            self.text_entry = tk.Entry(self.params_frame, width=30)
            self.text_entry.pack(side=tk.LEFT, padx=5)
    
    def search(self):
        root_path = self.path_var.get()
        if not root_path:
            messagebox.showwarning("Внимание", "Выберите папку для поиска")
            return
        
        search_type = self.strategy_var.get()
        
        for item in self.tree.get_children():
            self.tree.delete(item)
        
        self.search_btn.config(text="Поиск...", state="disabled")
        self.root.update()
        
        try:
            all_files = []
            for dirpath, _, filenames in os.walk(root_path):
                for filename in filenames:
                    all_files.append(os.path.join(dirpath, filename))
            
            results = []
            
            if search_type == "duplicate":
                hashes = {}
                for filepath in all_files:
                    try:
                        with open(filepath, 'rb') as f:
                            file_hash = hashlib.md5(f.read()).hexdigest()
                        if file_hash not in hashes:
                            hashes[file_hash] = []
                        hashes[file_hash].append(filepath)
                    except (PermissionError, OSError):
                        continue
                
                for file_hash, files in hashes.items():
                    if len(files) > 1:
                        for f in files:
                            results.append({'path': f, 'info': f"Хеш: {file_hash[:8]}..."})
            
            elif search_type == "old":
                cutoff = time.time() - (30 * 86400)
                for filepath in all_files:
                    try:
                        if os.path.getmtime(filepath) < cutoff:
                            days = (time.time() - os.path.getmtime(filepath)) // 86400
                            results.append({'path': filepath, 'info': f"{int(days)} дней назад"})
                    except (PermissionError, OSError):
                        continue
            
            elif search_type == "text":
                search_text = self.text_entry.get().lower()
                if not search_text:
                    messagebox.showwarning("Внимание", "Введите текст для поиска")
                    return
                text_ext = {'.txt', '.py', '.md', '.csv', '.json', '.xml', '.html'}
                for filepath in all_files:
                    if os.path.splitext(filepath)[1].lower() in text_ext:
                        try:
                            with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                                content = f.read().lower()
                                if search_text in content:
                                    lines = content.split('\n')
                                    for i, line in enumerate(lines):
                                        if search_text in line:
                                            line_preview = line[:80] + "..." if len(line) > 80 else line
                                            results.append({'path': filepath, 'info': f"Строка {i+1}: {line_preview}"})
                                            break
                        except (PermissionError, UnicodeDecodeError):
                            continue
            
            elif search_type == "size":
                min_bytes = 10 * 1024 * 1024
                for filepath in all_files:
                    try:
                        size = os.path.getsize(filepath)
                        if size >= min_bytes:
                            results.append({'path': filepath, 'info': f"{size / (1024*1024):.2f} MB"})
                    except (PermissionError, OSError):
                        continue
            
            for r in results:
                self.tree.insert("", "end", text=r['path'], values=(r.get('info', ''),))
            
            count = len(results)
            if count == 0:
                messagebox.showinfo("Результат", "Файлы не найдены")
            else:
                messagebox.showinfo("Завершено", f"Найдено {count} файлов\n")
                
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
        finally:
            self.search_btn.config(text="НАЙТИ", state="normal")


if __name__ == "__main__":
    root = tk.Tk()
    app = SearchApp(root)
    root.mainloop()