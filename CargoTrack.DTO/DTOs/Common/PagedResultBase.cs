namespace CargoTrack.DTO.DTOs.Common
{
    // Neden: Sayfalama bilgisi (kaçıncı sayfa, toplam kayıt vb.) her listede aynı. Ortak sınıf,
    // _Pagination bileşeni de bunu kullanır.
    public class PagedResultBase
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}