# SQLite veritabanını kontrol etmek için PowerShell scripti

try {
    # SQLite assembly'sini yükle
    Add-Type -Path "C:\Users\beratz\.nuget\packages\system.data.sqlite\1.0.119\lib\net6.0\System.Data.SQLite.dll"
    
    $connectionString = "Data Source=sefer_takip.db"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    Write-Host "Veritabanı bağlantısı başarılı!" -ForegroundColor Green
    
    # ID 455 ve 456'yı kontrol et
    $query = "SELECT Id, KalkisZamani, Durum FROM Seferler WHERE Id IN (455, 456)"
    $command = New-Object System.Data.SQLite.SQLiteCommand($query, $connection)
    $reader = $command.ExecuteReader()
    
    Write-Host "`nID 455 ve 456 seferleri:" -ForegroundColor Yellow
    $count = 0
    while ($reader.Read()) {
        Write-Host "ID: $($reader['Id']), Kalkış: $($reader['KalkisZamani']), Durum: $($reader['Durum'])"
        $count++
    }
    
    if ($count -eq 0) {
        Write-Host "ID 455 ve 456 ile sefer bulunamadı." -ForegroundColor Red
    }
    
    $reader.Close()
    
    # Toplam sefer sayısını kontrol et
    $countQuery = "SELECT COUNT(*) FROM Seferler"
    $command = New-Object System.Data.SQLite.SQLiteCommand($countQuery, $connection)
    $totalCount = $command.ExecuteScalar()
    Write-Host "`nToplam sefer sayısı: $totalCount" -ForegroundColor Cyan
    
    # En büyük ID'yi kontrol et
    $maxIdQuery = "SELECT MAX(Id) FROM Seferler"
    $command = New-Object System.Data.SQLite.SQLiteCommand($maxIdQuery, $connection)
    $maxId = $command.ExecuteScalar()
    Write-Host "En büyük sefer ID: $maxId" -ForegroundColor Cyan
    
    # 450'den büyük ID'leri listele
    $bigIdsQuery = "SELECT Id FROM Seferler WHERE Id > 450 ORDER BY Id"
    $command = New-Object System.Data.SQLite.SQLiteCommand($bigIdsQuery, $connection)
    $reader = $command.ExecuteReader()
    
    Write-Host "`n450'den büyük ID'ler:" -ForegroundColor Yellow
    $bigIds = @()
    while ($reader.Read()) {
        $bigIds += $reader['Id']
    }
    
    if ($bigIds.Count -gt 0) {
        Write-Host ($bigIds -join ", ")
    } else {
        Write-Host "450'den büyük ID bulunamadı." -ForegroundColor Red
    }
    
    $reader.Close()
    $connection.Close()
    
} catch {
    Write-Host "Hata: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack Trace: $($_.Exception.StackTrace)" -ForegroundColor Red
}