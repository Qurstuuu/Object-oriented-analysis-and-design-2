import os
import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import subprocess
import platform
from strategies import DuplicateFinder, OldFileSearcher, TextInFileSearcher, SizeBasedSearcher

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
        
        if search_type == "text":
            if not hasattr(self, 'text_entry') or not self.text_entry.get():
                messagebox.showwarning("Внимание", "Введите текст для поиска")
                return
        
        for item in self.tree.get_children():
            self.tree.delete(item)
        
        self.search_btn.config(text="Поиск...", state="disabled")
        self.root.update()
        
        try:
            if search_type == "duplicate":
                searcher = DuplicateFinder()
            elif search_type == "old":
                searcher = OldFileSearcher(days_old=30)
            elif search_type == "text":
                search_text = self.text_entry.get()
                searcher = TextInFileSearcher(search_text, case_sensitive=False)
            elif search_type == "size":
                searcher = SizeBasedSearcher(min_mb=10)
            else:
                return
            
            results = searcher.search(root_path)
            
            self.display_results(results)
            
        except Exception as e:
            messagebox.showerror("Error", str(e))
        finally:
            self.search_btn.config(text="Поиск", state="normal")
    
    def display_results(self, results):
        count = 0
        
        for result in results:
            if isinstance(result, list):
                for item in result:
                    info = item.get('info', item.get('hash', ''))
                    self.tree.insert("", "end", text=item['path'], values=(info,))
                    count += 1
            elif isinstance(result, dict):
                info = result.get('info', '')
                self.tree.insert("", "end", text=result['path'], values=(info,))
                count += 1
        
        if count == 0:
            messagebox.showinfo("Результат", "Файлы не найдены")
        else:
            messagebox.showinfo("Завершено", f"Найдено {count} файлов\n")

if __name__ == "__main__":
    root = tk.Tk()
    app = SearchApp(root)
    root.mainloop()