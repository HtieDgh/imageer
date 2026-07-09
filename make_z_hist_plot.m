clear
close all hidden
clc

pkg load io
%% Извлечение данных из cvs файла
  tmp=csv2cell("hist.csv");
  pixels = cell2mat(tmp(1:end-1));
  figureTitle=cell2mat(tmp(end));% Получить название графика

h = figure();
  hist(pixels,[0:255]);
  set(h,'name',sprintf("%s",figureTitle));
  grid on
  ax = gca();  % Получить текущие оси
  set(ax, 'XLim', [-1, 256]);  % Установить пределы X
  set(ax, 'XTick', 0:8:256);   % Основные деления с шагом 0.5
  title(sprintf("Распределение яркостей, кол-во пикселей = %i", length(pixels)),
      'FontSize', 16           % Размер шрифта
      );
  xlabel('Z');
  ylabel('Количество');
%% Ожидание пока график не будет закрыт
waitfor(h)