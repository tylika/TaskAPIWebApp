using System.ComponentModel.DataAnnotations;
// Якщо будемо реалізовувати завантаження файлів, тут може знадобитися IFormFile,
// але для поточного контролера, який приймає FilePath, DTO буде простим.
// Для реального завантаження, DTO для POST міг би бути іншим,
// наприклад, містити TaskId та IFormFile.

namespace TaskAPIWebApp.Models // Або TaskAPIWebApp.Models.Dtos
{
    public class TaskAttachmentInputDto
    {
        [Required(ErrorMessage = "ID Завдання є обов'язковим.")]
        public int TaskId { get; set; }

        // Це поле буде заповнюватися на сервері після завантаження файлу.
        // Для поточного контролера, який приймає FilePath, залишимо його тут.
        // В реальному сценарії з IFormFile, це поле б не надходило від клієнта.
        [Required(ErrorMessage = "Шлях до файлу є обов'язковим (для поточної імплементації).")]
        [StringLength(1000, ErrorMessage = "Шлях до файлу не може перевищувати 1000 символів.")]
        public string FilePath { get; set; }
    }

    public class TaskAttachmentUpdateDto // DTO для оновлення, якщо потрібно
    {
        // Зазвичай оновлювати FilePath для існуючого вкладення може бути небажано,
        // можливо, краще видалити старе і додати нове.
        // Але якщо логіка дозволяє, можна додати поля для оновлення.
        // Наприклад, можна було б оновити опис файлу, якщо б таке поле було.
        // Для поточного контролера, він приймає весь об'єкт TaskAttachment.
        [Required]
        public string FilePath { get; set; }
    }
}