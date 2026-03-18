**Téma:** Osobní správce financí  
**Varianta:** (a) Jednoduchá databázová aplikace  
•	**sledované entity:** Kategorie, Šablony plateb (předplatná/výplaty), Transakce, Spořicí cíle, Vklady na cíle, Vygenerovaný měsíc  
•	každá Transakce i Šablona je zařazena do jedné Kategorie a má jasně určený typ (příjem/výdaj).  
•	Šablony slouží k hromadnému generování reálných Transakcí na začátku měsíce (chrání se tím historická data před zpětnou úpravou cen).  
•	Entita Vygenerovaný měsíc slouží jako pojistka, která zabraňuje duplicitnímu vygenerování šablon při opakovaném spuštění aplikace.  
•	každý Spořicí cíl (např. auto, telefon) má určenou cílovou částku a je napojen na více Vkladů. Vklad peněz na cíl automaticky generuje i odpovídající výdajovou Transakci v rozpočtu.  
•	aplikace umožňuje uživateli vytvářet, prohlížet, editovat a mazat veškeré uvedené typy entit  
•	aplikace vizuálně zobrazuje postup u Spořicích cílů  
•	pro perzistenci dat je využita embedded knihovna LiteDB  
