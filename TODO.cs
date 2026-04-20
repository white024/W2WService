/*TODO: proje hakkında.....
 
Gateway
Önbellekleme
Load ballancing
Auth geliştirmelri
AuthService Checklist
Kullanıcı İşlemleri

 Kullanıcı bilgilerini güncelle (ad, soyad, telefon)
 Şifre değiştir (eski şifre doğrulama ile)
 Şifremi unuttum (email ile reset token gönder)
 Şifre reset (token ile yeni şifre set et)
 Kullanıcıyı getir (profil)

Company İşlemleri

 Invite sistemi test et
 Invite iptal et
 Invite listele (kullanıcı bazlı)
 Şirketten kullanıcıyı çıkar
 Şirketteki kullanıcıları listele
 Kullanıcının rol güncelleme (Viewer → Admin vb.)

Aggregation / Listeleme

 Kullanıcı listesi (filtre: şirket, rol, aktiflik, sayfalama)
 Şirket listesi (filtre: isim, sayfalama)
 Şirkete ait kullanıcı listesi (rol bazlı)
 Kullanıcının şirketleri (aktif/pasif)
 Kullanıcı arama (isim, email, telefon)
 Şirket bazlı istatistik (kullanıcı sayısı, roller dağılımı)

Güvenlik

 Brute force koruması (login denemesi limitle)
 Rate limiting (Gateway'de)
 Token reuse detection (revoke edilmiş token tekrar kullanılırsa tüm sessionları kapat)
 Şüpheli login bildirimi (farklı IP, farklı cihaz)

Gateway

 Rate limiting
 Request/Response loglama
 Tüm servis route'ları
 Health check endpoint'leri

Genel

 Unit testler (kritik business logic)
 Integration testler
 Hata mesajlarını standartlaştır
 Notification servisi (email, SMS)
 Tüm exception'ları gözden geçir

--------------------------------------
[
 Auth
 Company
 catalog
 cart
 order
 payment
 history
]
servisleri olacak web api
[
 Virtualpos
 Shared
]
class library yardımcı alanlar olacak kafka ve ortak alanlar sanal pos işlemleri için


Auth, Company -> mongo (tek db tek collation auth and companies sub collation) olacak
Cart -> mongo olacak
History -> mongo olacak - order, order change, product, price, categoyr, unit, user, company, cart history kaydı tutacak
Catalog -> ms sql olacak
order, payment -> ms sql tek db olacak hareketler vb tek yerde olacak servisleri ayrı olacak
birde log servisi olacak tüm servislerin ortak loğlarını mongoya date : lgs tarzı yazacak seq olabilir
kafka ve seq kullanacağım
natification service de eklenecek ek olarak ama ona karar veremedim
redis
elesticsearch sonra eklenebilir
sharedde loğa gerek var mı bilemedim log ile ilgili bir şeye

------------------------------------
DTOs dtolar yine tek yerde olsun istedim ama bilemedim böyle mi olmalı
Enums tüm enumlar service bazlı klasör yapısı
Events bilemedim nasıl olmalı sen buna örnek ver yada sileyim mi bide kafka için falan gerekir mi
Extentions Logextention gibi ek yapılar için düşündüm
Kafka kafka işlemleri ama nasıl olacak bilemedim basit generic bir yapı olsa çok iyi olur aslında
Middleware yine log vb için gerekir mi bilemedim yada benzeri işlemler için gerçi servis varken gerekir mi ki
Models generic return object vb için
Repostries generic işlemler için
Services generic işlemler için
*/